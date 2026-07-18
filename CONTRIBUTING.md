# Contributing

Sylves is a one-person project currently, but bug reports and fixes are welcomed. Please use the github UI for submission.

You are welcome to propose new features, but discuss anything with me before implementing.


# Releasing Sylves

Notes on cutting a release of Sylves.

Sylves
* Run tests
* Choose new version
* Update docs/articles/release_notes.md
* Update Version in src/Sylves/Sylves.csproj
* Run `release.py`
* Push to github
* Publish docs to website
* Create github release of zips in release/, tag main branch
* Run upm_release.py
* Commit, updating branch `upm` and adding tag `upm/vx.x.x`, then push
* Build / Publish to NuGet:
  * `dotnet pack -c Release src/Sylves/Sylves.csproj`
  * `dotnet nuget push src/Sylves/bin/Release/Sylves.<version>.nupkg -k <api-key> -s https://api.nuget.org/v3/index.json`

Sylves demos
* Update sylves-demos
* Build
* Push to github
* Upload to itch.io