Thanks that you want to help! Please read the sections below.

## Contributing (General)

If you want to send a bug report just open a new issue!

Otherwise just fork the project an send a pull request to discuss and merge the changes.
If you want to discuss the changes beforehand you can open a new issue.
Even better when there is already an open issue marked as [up-for-grabs](https://github.com/Antaris/RazorEngine/labels/up-for-grabs).
Please send the pull request against the `master` branch and mention an already existing issue (if present).

This project is searching for new maintainers, so if you want to help write on gitter or start sending PRs :)

## Contributing Documentation

As the documentation is generated from the repository you can help improving the documentation by editing the files in the `/doc` folder.
You can even edit a page directly on github by clicking the edit button ([for example this page](https://github.com/Antaris/RazorEngine/blob/master/CONTRIBUTING.md)).
See also https://help.github.com/articles/editing-files-in-your-repository/ 
(don't forget to send a pull request back after forking and changing something).

## Branching model

The branching model in http://nvie.com/posts/a-successful-git-branching-model/ is used (which you do not need to read to send a pull request).
However the naming differs: `develop` is called `master` and `master` is called `releases` in RazorEngine.

## Licensing

This project is subject to the terms and conditions defined in file ['LICENSE.md'](https://github.com/Antaris/RazorEngine/blob/master/LICENSE.md), which is part of this source code package.

You can find licenses of the programs this project depends on in either the "lib/$Project" folder or on their nuget page.


## Releasing

1. Update `ReleaseNotes.md`
2. Update `buildConfig.fsx` (versions)
   Roslyn Package versions only need to increased when we changed public API or when we made changes there.


3. Run `yaaf_merge_master=true PUSH_ROSLYN=true nugetkey=<accesskey> ./build.sh Release`
   - `yaaf_merge_master` -> can be used on build servers to force-switch on develop branch for the version bump commit.
   - `PUSH_ROSLYN` -> push the roslyn packages (if you increased their versions above)
