This fork includes some significant cleanups, optimizations, and reworks of BetterInfestations! Changes result in noticeable tps improvements at high pawn/hive counts, and should help with code maintainability.

### Changes:
- Added combat strength limits for map, hive, and hive groups (200 defense, 400 hunting) to limit number of insects (still plenty to go around!)
- Consolidated number of hunter groups from 3 -> 2
- Code cleanup and performance improvements

### Issues:
- Insectoids would keep spawning up to a theoretical limit of 45/hive (up to ~2300 with max hives)!
  - Solution:
    - Added combat strength limits and reduced number of hunter groups
- JobGiver_Patrol fires on every tick as last thinknode in BI_HiveHunters duty
  - Solution:
    - Gated behind ThinkNode_PerTick to fire every 10 seconds for individual pawns
- Incredible lag if no more prey found on map
  - Solution:
    - Cleaned up search process for finding prey and set distance limits on map search
- Generally expensive functions to track pawn/hive relationship and hive boundaries 
  - Solution:
    -  Cached pawn/hive assignment and cells within hive via new MapComponent
