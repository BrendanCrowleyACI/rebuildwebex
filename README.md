# rebuildwebex
Webex Rebuild Tool

The Webex Rebuild Tool rebuilds a streamed webex recording as a .arf file that can be then played offline for later viewing, it's based upon https://github.com/skuater/rebuildwebex (written in Spanish). My solution leverages this tool but has been improved, translated to English plus a few bugs fixed :)

How to use:
1. Click on the Play recording link in the email with subject "recording of today's session ..."
2. This opens the WebEx Network Recording Player, wait until video will be downloaded. There is a blue buffering indicator and it should be 100%
3. Copy contents of WebEx temp folder (**%USERPROFILE%\AppData\Local\Temp\<several digits>**) to a temporary location (e.g. **C:\Temp\rebuild**) when full recording is downloaded
4. Download and execute **[rebuild.exe](https://github.com/BrendanCrowleyACI/rebuildwebex/blob/master/exe/rebuild.exe)**, provide temporary location from Step 3 into field Directory: and press Run button
5. Open resulted rebuild.arf file located in the temporary location with the WebEx Network Recording Player
6. **Optional:** Convert video to appropriate format for offline viewing: File > Convert Format (usually *.wmv or *.mp4)
