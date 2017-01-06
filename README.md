# Steamless

Steamless is a DRM remover of the SteamStub variants.

The goal of Steamless is to make a single solution for unpacking all Steam DRM packed files. Steamless aims to support as many games as possible.<br>
However, due to personal limited funds, I cannot test every game myself.

# What is SteamStub DRM?

```
Steamworks Digital Rights Management wraps your game's compiled executable and checks to make sure that it is running under an authenticated instance of Steam. This DRM solution is the same as the one used to protect games like Half-Life 2 and Counter-Strike: Source. Steamworks DRM has been heavily road-tested and is customer-friendly. 
In addition to DRM solutions, Steamworks also offers protection for game through day one release by shipping encrypted media to stores worldwide. There's no worry that your game will leak early from the manufacturing path, because your game stays encrypted until the moment you decide to release it. This protection can be added to your game simply by handing us finished bits or a gold master. 

ref: hxxps://partner.steamgames.com/documentation/api
```

# Supported Versions

Steamless currently supports the following SteamStub DRM variants:

  * **SteamStub Variant 1**
    * There is currently no support for this version of the DRM.
  * **SteamStub Variant 2**
    * 32bit version of this variant is supported.
  * **SteamStub Variant 3**
    * **Variant 3.0.0**
      * 32bit version of this variant is supported.
    * **Variant 3.0.1**
      * 32bit version of this variant is supported.

*Please note; these version numbers are superficial. They are an assumed version based on major changes to the DRM over its lifespan.*

# Legal

```
Steamless is released under the following license:
Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International

Unless otherwise separately undertaken by the Licensor, to the extent possible, the Licensor offers the Licensed Material 
as-is and as-available, and makes no representations or warranties of any kind concerning the Licensed Material, whether 
express, implied, statutory, or other. This includes, without limitation, warranties of title, merchantability, fitness 
for a particular purpose, non-infringement, absence of latent or other defects, accuracy, or the presence or absence of 
errors, whether or not known or discoverable. Where disclaimers of warranties are not allowed in full or in part, this 
disclaimer may not apply to You.

Steamless is not intended for malicious use or for the use of obtaining or playing games illegally.
Steamless should only be used on games that you legally purchased and own.

Steamless is not associated with Steam or any of its partners / affiliates.
No code used within Steamless is taken from Valve or any of its partners / affiliates.

Steamless is released for educational purposes in the hopes to learn and understand DRM technologies. 

Use Steamless at your own risk. I, atom0s, am not responsible for what happens while using Steamless. You take full reponsibility for any outcome that happens to you while using this application. Do not distribute unpacked files.
```

# Thanks

Thanks to Cyanic (aka Golem_x86) for his notes and help with parts of the stub headers and such.<br>
You can find his information here: http://pcgamingwiki.com/wiki/User:Cyanic/Steam_DRM

# Compiling Steamless

Steamless is coded using Visual Studio 2015 (Update 3) at this time.<br>
To compile, you should only need to load the sln file and compile as-is.

No changes should be needed to the solution or source.

# Contributing To Steamless (Guidelines)

I welcome and encourage contributions to the Steamless project. However, I do have some guidelines I wish for people to follow when doing so.

  * Please follow the similar coding style / naming conventions found in Steamless already.
  * Please do not use tabs. Tabs should be 4 spaces instead.  
  * Please do not introduce additional dependencies without a discussion before hand.
  * Please do not alter or remove any copyrights without a discussion prior.
  * Please do not hard code information specific to any one target. Steamless should be dynamic full all titles.

Discussions can be opened within the Issue tracker here:
  * https://github.com/atom0s/Steamless/issues