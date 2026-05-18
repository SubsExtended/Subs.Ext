Rating.WPF application is a part of solution that will selectively display subtitles for audio/video media, depending on
a) how user rates his skill with the language of media.
b) how each phrase difficulty is rated in subtitles file. prases' difficulty can be rated
judging by words frequency in the language, or how clearly the phrase is pronounced in the media.

The goal of this solution is to see/listen media mainly in its original language.
But if some phrases are rated as hard for understanding (hearing) - media player will display subtitles for that phrase.

Another part of this solution will be extensions for variuos media players, desktop and web based.
Intended workflow of such extensions:
1. User opens a media file in media player.
2. New menu option is added to the media player:
"Select your language level". Options are: A, B, C, D, E.
3. User opens a corresponding modified subtitles file created with this application.
Modification will include adding a rating A/B/C/D/E after each subtitle line.
If "Select your language level" is lower than such rating - media player will display the subtitle line.
If it is equal or lower - media player will not display the subtitle line.
(For now, this application will allow to open only *.SRT files and make a modified copy of it.)

If I see enough interest in this project, I will add support for more media formats and more subtitle formats,
and will create addins for popular media players.

Also, it is possible to connect to AI services to automatically rate phrases in subtitles files, and to create a web service for sharing such files between users.

WIP: VLC extension
WIP: Windows Media Player extension
WIP: Chrome extension
WIP: Edge extension