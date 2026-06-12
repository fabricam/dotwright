Dotwright — CI and local Playwright demo

This repository now includes a Playwright test suite and a simple ingestion demo that stores test results into a local SQLite database for the dashboard.

Local run (Windows PowerShell):

1. Install Node and Python.
2. From repository root, run:
   .\scripts\run-playwright.ps1

Run the GitHub Actions workflow locally with act (Linux image) or the official runner:

- Using act (install from https://github.com/nektos/act):
  act -j playwright -P ubuntu-latest=nektos/act-environments-ubuntu:18.04

- Using GitHub Actions Runner: follow docs to configure a self-hosted runner and run the workflow file .github/workflows/squad-playwright.yml.

Note: This branch is squad/backlog-setup-ci. Do NOT push without Keaton's OK.
