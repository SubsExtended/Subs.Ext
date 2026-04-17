This application is a part of solution that will selectively display subtitles for audio/video media, depending on
a) how user rates his skill with the language of media.
b) how each phrase difficulty is rated in subtitles file. prases' difficulty can be rated
judging by words frequency in the language, and by how clearly the phrase is pronounced in the media.

The goal of this solution is to see/listen media mainly in its original language.
But if some phrases are rated as hard for understanding (hearing) - media player will display subtitles for that phrase.

Another part of this solution will be an addin for variuos media players.

Intended workflow:
1. User opens a media file in media player.
2. New menu option added to the media player, in subtitles section:
"Select your language level". Options are: A, B, C, D, E.
3. User opens a proprietary subtitles file created with this application
For the beginning, this application will allow to open only *.SRT files and make a modified copy of it.
Modification, made by application users, will include adding a rating A/B/C/D/E after each subtitles line.
If "Select your language level" is lower than such rating - media player will display subtitles.