# CodeFactory Packages
Repository for ready to use packaged automation for CodeFactory, along with full source code.

## Overview
All the CodeFactory automation that is hosted in this repository is designed to meet the following needs.
- Ready to use right away no customization is required.
- Can be copied and extended or changed to meet whatever needs for your delivery involves.
- Training aid by providing live code that can be review and used to show how different parts of the SDK are used.

## Automation Packages
Automation packages contain the core logic that makes up the pre built automation used with implementation of CodeFactory commands.

|PackageName|Description|Ready to Use|Status|
|--|--|--|--|--|--|
|CodeFactory.Automation.Standard.Logic|Standard automation libraries that are usable on any project.|Yes|Initial Release|
|CodeFactory.Automation.NDF.Logic|Automation logic for a number of delivery scenarios that use the CodeFactory.NDF library in the implementation.|Yes|Initial Release|


## Architecture Implementation

Architecture implementation focuses on target implementations of CodeFactory automation. Each implementation is ready to use out of the box and consumes the already defined automation packages.

|PackageName|Description|Ready to Use|Status|
|--|--|--|--|
|CodeFactory.Architecture.AspNetCore.Service.Rest|Full architecture automation to support automation from consuming a EF SQL implementation through the generation of WebAPI rest based API services.|Yes|Initial Release|
|CodeFactory.Architecture.Blazor.Server|Full architecture automation to support automation from EF SQL implementation through services to complete blazor server component implementation with automation.|Yes|Initial Release|

