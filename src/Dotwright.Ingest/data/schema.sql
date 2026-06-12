-- SQLite schema for Dotwright ingest
CREATE TABLE IF NOT EXISTS runs (
  id TEXT PRIMARY KEY,
  project TEXT,
  started_at DATETIME,
  finished_at DATETIME,
  status TEXT,
  raw_payload TEXT
);

CREATE TABLE IF NOT EXISTS test_results (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  run_id TEXT NOT NULL,
  test_name TEXT,
  status TEXT,
  duration_ms INTEGER,
  error_message TEXT,
  timestamp DATETIME,
  FOREIGN KEY(run_id) REFERENCES runs(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_test_results_run ON test_results(run_id);
