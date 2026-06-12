#!/usr/bin/env python3
"""Simple ingestion sample: read Playwright JSON reporter output and store into SQLite for demo dashboard."""
import argparse
import json
import sqlite3
from datetime import datetime
import sys


def collect_tests(obj, out):
    if isinstance(obj, dict):
        if 'title' in obj and 'status' in obj:
            out.append(obj)
        for v in obj.values():
            collect_tests(v, out)
    elif isinstance(obj, list):
        for item in obj:
            collect_tests(item, out)


def main():
    p = argparse.ArgumentParser(description='Ingest Playwright JSON results into SQLite')
    p.add_argument('--input', '-i', required=True, help='Path to Playwright JSON results')
    p.add_argument('--db', '-d', default='ingestion.db', help='SQLite DB path')
    args = p.parse_args()

    try:
        with open(args.input, 'r', encoding='utf-8') as f:
            data = json.load(f)
    except Exception as e:
        print(f'Failed to read input: {e}', file=sys.stderr)
        sys.exit(2)

    tests = []
    collect_tests(data, tests)

    conn = sqlite3.connect(args.db)
    cur = conn.cursor()
    cur.execute('''
    CREATE TABLE IF NOT EXISTS test_results (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        test_name TEXT,
        status TEXT,
        duration_ms INTEGER,
        error TEXT,
        recorded_at TEXT
    )
    ''')

    now = datetime.utcnow().isoformat() + 'Z'
    inserted = 0
    for t in tests:
        name = t.get('title')
        status = t.get('status')
        duration = t.get('duration') or t.get('durationMs') or None
        err = None
        if 'error' in t and t['error']:
            err = t['error'].get('message') if isinstance(t['error'], dict) else str(t['error'])
        cur.execute('INSERT INTO test_results (test_name, status, duration_ms, error, recorded_at) VALUES (?,?,?,?,?)',
                    (name, status, duration, err, now))
        inserted += 1

    conn.commit()
    conn.close()
    print(f'Inserted {inserted} test rows into {args.db}')


if __name__ == '__main__':
    main()
