# OSTRename
A little tool to rename and number files of an OST

## Layout of a translation file
### Header
The header is made of 4 lines:
 - 1st line: The beginning, a line of ```=```
 - 2nd line: The title of the OST
 - 3rd line: The pattern of the translations: ```⇒``` at the beginning indicates that the files can be numbered, then ```Lang1 ⇔ Lang2 ⇔ Lang3``` with as much languages as you want
 - 4th line: The end, a line of ```=```
 - An empty line between the header ant the translations part
#### Example of header:
 ```
=======================================================================================
Mario & Luigi: Dream Team Bros
⇒ System ⇔ English ⇔ Français
=======================================================================================
 ```
 ### Translations
 The translations part contains one line per translation with the pattern given in the header. Logically the 1st translation is written in the 6th line of the file.
 #### Example of translations part:
 ```
 38 ⇒ STRBGM_DREAM_PAJAMAU_MASTER ⇔ Glorious Pajamaja Dreams ⇔ Rêves au sommet
44 ⇒ STRBGM_DREAM_ZAKOBTL_MASTER ⇔ Victory in the Dream World ⇔ Victoire onirique
51 ⇒ STRBGM_ENDING_MASTER ⇔ Memories of Pi'illo Island ⇔ Souvenirs de l'île Koussinos
3 ⇒ STRBGM_HAPPY ⇔ Enjoy The Joy! ⇔ Joyeuse volupté
26 ⇒ STRBGM_HIRUNEBEACH_MASTER ⇔ Sunny Driftwood Shore ⇔ Soleil sur la plage Lidemer
9 ⇒ STRBGM_KUPPATHEME ⇔ Bowser's Theme ⇔ Thème de Bowser
48 ⇒ STRBGM_LASTBOSS_MASTER ⇔ Adventure's End ⇔ L'aventure se termine
2 ⇒ STRBGM_LOAD ⇔ Travel Journal ⇔ Carnet de voyage
22 ⇒ STRBGM_MADOROMI ⇔ Dozing Sands Secret ⇔ Mystérieux désert
 ```
