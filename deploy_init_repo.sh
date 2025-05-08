#!/usr/bin/env bash
# Deployment helper for project initialization
# -------------------------------------------
# 1. Initializes a local git repository (if none exists)
# 2. Commits all current files
# 3. Tags the initial version (v0.0.1)
# 4. Creates a matching public GitHub repository named "USAA-Camera-Control"
#    using the GitHub REST API (requires a Personal Access Token).
# 5. Pushes the code and tags to GitHub.
#
# Usage:
#   bash deploy_init_repo.sh <github-username> <github-token>
#
# You need a GitHub token with "repo" scope. Create one at:
#   https://github.com/settings/tokens
#
# -------------------------------------------
set -euo pipefail

REPO_NAME="USAA-Camera-Control"
VERSION_TAG="v0.0.1"

if [[ $# -lt 2 ]]; then
  echo "Usage: $0 <github-username> <github-token>" >&2
  exit 1
fi

GH_USER="$1"
GH_TOKEN="$2"

# Initialize git repository if absent
if [[ ! -d .git ]]; then
  echo "Initializing git repository..."
  git init
fi

# Stage and commit everything
if [[ -n "$(git status --porcelain)" ]]; then
  echo "Creating initial commit..."
  git add .
  git commit -m "Initial commit of ${REPO_NAME} ${VERSION_TAG}"
fi

# Create/replace version tag
if git rev-parse "${VERSION_TAG}" >/dev/null 2>&1; then
  git tag -d "${VERSION_TAG}"
fi
git tag "${VERSION_TAG}"

echo "Ensuring GitHub repository ${REPO_NAME} exists..."

# Create repo via GitHub API (ignore if already exists)
curl -s -o /dev/null -w "%{http_code}" \
     -u "${GH_USER}:${GH_TOKEN}" \
     -X POST \
     -H "Accept: application/vnd.github+json" \
     https://api.github.com/user/repos \
     -d "{\"name\":\"${REPO_NAME}\", \"private\":false}" | \
  { read -r status; [[ "$status" == "201" || "$status" == "422" ]] || { echo "GitHub repo creation failed (status $status)"; exit 1; }; }

# Add remote if absent
if ! git remote get-url origin >/dev/null 2>&1; then
  git remote add origin "https://github.com/${GH_USER}/${REPO_NAME}.git"
fi

echo "Pushing code and tags to GitHub..."
# Push default branch and tags
CURRENT_BRANCH="$(git symbolic-ref --short HEAD)"

git push -u origin "$CURRENT_BRANCH"
git push origin "${VERSION_TAG}"

echo "Deployment complete!" 