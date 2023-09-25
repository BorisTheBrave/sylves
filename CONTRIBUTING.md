# Contributing

Sylves is a one-person project currently, but bug reports and fixes are welcomed. Please use the github UI for submission.

You are welcome to propose new features, but discuss anything with me before implementing.


# Releasing Sylves

Notes on cutting a release of Sylves.

Sylves
* Update release_notes.md
* run `release.py`
* Push to github
* Publish docs to website
* Create github release of zips in release/, tag main branch
* Run upm_release.py
* Commit, push and tag upm branch `upm/vx.x.x`

Sylves demos
* Update sylves-demos
* Build
* Push to github
* Upload to itch.io