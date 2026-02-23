* detect credits
  * easier said than done, not particularly standardized. from what i can tell the three main ways
  credits are implemented are:
    1. a single event with a format along the lines of `Role: Name\N`
    2. a single event with a format along the lines of `Role\NName\N\N`
    3. two events, one is `Role:\N` and the other, position a bit to the right, is `Name\N`
  * now of course these can be across multiple events etc
  * something to sort later™
* ignore credits when flagging as typeset
* extract project garbage
* Check for malformed ass files, e.g. double \[Script Info\], styles that don't exist,
* meta analysis on multiple sub files
  * check if any subs are outliers in sign count, both TS'd and generally
* check sign analysis on non latin languages
* count overlaps for signs (non-core languages seem to have like, 1 overlap on a heavy TS show. doesn't really count imo)
* add Text to the actor check on sign detection
* check actor fields exist (sign of file not being in-house)
* [CR] The Villainess Is Adored by the Prince of the Neighbor Kingdom - S01E06 [00p].07.ara.Arabic (Saudi Arabia).ass
  ^ check font detection, TimesNewRoman doesnt seem legit to me. CLI issue?