# EnginePrimeSync
This is a command line tool with various utilities to help you manage your Denon Engine Prime database.
I wrote this because I was frustrated with various shortcomings of the GUI app (as of this date, 8/19/21)
such as the inability to import playlists/crates from external drives, inability to wholesale import
an external database to overwrite your local copy (useful if you're restoring a backup or just want
everything to match what you did on the devices), ability to remap paths where files are stored in case
you move your db location or the files, etc.

# REQUIREMENTS
If you're exporting to an external drive, **THE DRIVE MUST BE FORMATTED AS exFat!** This is because exFat
supports Unicode characters in file and directory names. FAT32 does not. Also there's just no reason
to use FAT32 anyway, every operating system can read exFat.

# COMPATIBILITY
This is designed to only work with edits done via the native Engine Prime app and the SC5000/6000
turntables themselves (i.e. working on the device itself). **I do not use nor care about Traktor,
iTunes, Serrato, etc. Thus, this tool will only operate on the main database files.** These files are:

m.db - aka "Main DB", holds most of your track and path information
p.db - aka "Performance DB" holds things like cues/loops, waveform analysis, etc.

**DON'T USE THIS IF YOU USE OTHER SOFTWARE TO MANAGE YOUR LIBRARIES.**

# WARNING READ THIS IF YOU READ NOTHING ELSE
This tool is not guaranteed to be 100% bug free. I think it is, because I've tested it on my own libraries.
But you may have a setup that I didn't expect or plan for (I wrote this on Windows, haven't tested on other platforms
even though the code is platform independent). It also overwrites existing data, that being databases, and in 
the case of a wholesale export to an external drive, the music that was previously on the external drive.
Also, as stated above, **this only works for databases made/edited with EP or the SC5/6000 turntables or 
whatever else runs Engine Prime on it. It will NOT work with Serato, iTunes, Traktor, Notepad, World of Warcraft,
or anything else**.

# TL;DR MAKE A BACKUP OF YOUR LIBRARY FILES
Specifically what you want to back up are "m.db" and "p.db" located wherever your "Engine Library"
folder is. This will generally be in your music directory (at least on Windows), and will be in the
root directory on external drives. **FOR EXTERNAL DRIVES YOU MAY ALSO WANT TO BACK UP THE ENTIRE
MUSIC SUBDIRECTORY. This folder contains your remapped music files.**

# Exporting DB Notes
I create the Music directory structure differently on the external drive than the Engine Prime software
does. Let's say on your PC you have your music files stored at: 

D:\MyMusic\Artist - Album Name\Track1.flac etc.

Engine Prime will create the directory in the following way (assuming external drive is labeled F:):

F:\Music\Album Artist (may differ from artist)\Album Name\Track1.flac etc

FURTHERMORE, because it writes assuming the lowest common denominator (FAT32), __it strips all Unicode and
special characters. Periods become underscores, umlauts become something random, etc.__

**This app straight up mirrors your original directory structure to the external drive. Unicode file names and all.**
So with the above example, it'd look like this with my app:

F:\Music\Artist - Album Name\Track1.flac etc
Exactly how you had it stored on your drive.

# USAGE GUIDE
I've tried to make the app display the info intelligently when you use it, and I've tried to make it
fault tolerant, i.e. if you enter the wrong values it doesn't let you proceed and just asks for input
again. **BACK UP YOUR SHIT BEFORE YOU PROCEED**. Anyway, let's go through each option:

# 1. Import/Export entire Database
Use this to WHOLESALE copy an ENTIRE database either from your computer to an external drive,
or IMPORT COMPLETELY a database from an external drive. Let's go through both options:

1. EXPORT LOCAL db to EXTERNAL DRIVE (will copy music files too)
2. IMPORT database FROM EXTERNAL DRIVE (doesn't copy music)

## Option 1.1 Guide For exporting entire Database:
This will completely wipe your external hard drive's Music folder and the m.db and p.db files located there. **This option
is meant to do a total mirror of what you have on your PC.** Upon choosing this, it attemps to auto find where your database
files are stored and will show you this path if it does. If it doesn't, you are prompted to enter a path. Regardless,
you have the option to change the path from the default. **Enter The FULL path to the folder containing m.db/p.db. Trailing
slash doesn't matter in this case**. For example, if your m.db file is located at D:\DropBox\Music\Engine Library\m.db you'd
enter: D:\DropBox\Music\Engine Library

The program will check to see if those files exist. If they don't, you'll be placed in a loop where you keep getting asked
for the proper folder until it's able to find the database files it needs. You can't screw this part up.

Next it asks you for the **destination drive LETTER** of the external drive. If you external drive is F:, you can enter any of the following:
F, F:, F:\\
I suppose this doesn't actually have to be removable media like a thumb drive. It could be any drive. If it's a network drive you'll
have to map it though first so that you can assign it a drive letter. 

Immediately after this, the app reads both your m.db and p.db files building all the internal data structures it needs. It does this
silently. Only if there is an error will you get something printed to the screen. if there's an error reading your DB, the app
will abort and return you to the main menu.

**THIS NEXT STEP, IN RED LETTERS, INFORMS YOU THAT UPON PRESSING ENTER IT WILL DELETE THE ENTIRE MUSIC FOLDER AND ITS CONTENTS ON THE
DESTINATION DRIVE!!! IF YOU HAVE NOT BACKED UP YET, DO SO NOW! ALSO BE DAMN SURE YOU ENTERED THE CORRECT DRIVE LETTER!**
You can either hit CTRL-C (on PC at least, not sure about Mac) or just close the app if you're scared and want to quit.

Next, the app will copy all the music found in your source database to your destination drive.
Here's the thing: the paths that are stored in the database on your PC are different than those stored
on the external drive. **Paths are also stored relative to the location of the database files.** As an example, I have my
music stored in D:\DropBox\Music\Archived Vinyl. My Engine Prime database is located at D:\DropBox\Music\Engine Library.
My tracks, as stored in my source database, appear with this path:

../Archived Vinyl/\<track directory and filename\>

If you aren't familiar with relative paths, please Google it. the ".." means "go up a directory". So in the above example,
it means that the tracks are located "up a directory" from D:\DropBox\Music\Engine Library, and then inside the 
"Archived Vinyl" subdirectory. The full path to this "Archived Vinyl" directory, in my case, is actually:
D:\DropBox\Music\Archived Vinyl\. Anyway that's it for my quick explanation of relative paths.

On the destination drive however, all music data has to be placed in the Engine Library\Music folder. So we have to remap
the prefix or root directory that contains the tracks from the source drive to the destination drive. _During this file
copy stage, the first time we encounter a track that we haven't found a matching prefix for that we need to remap,
the app will prompt you with the following:_

	Please enter the prefix you wish to replace from the string below **(DON'T include trailing slash)**:
	../Archived Vinyl/SomeArtist - SomeAlbum/SomeTrack.mp3

What we need to do here is choose the text to **STRIP OUT** from that path. That way the app can create the new
and correct path on the external drive. Enter the top level directory that contains all your music.
**DON'T INCLUDE THE TRAILING SLASH**. So in this case, for me, I'd enter:

../Archived Vinyl

Don't be scared: what happens next is that we validate your input. _If you mistyped something and we can't find what
you entered at the start of the path's string, we'll prompt you again._

You then get another confirmation prompt. This will show you the path we are converting FROM, aka the source db path,
and what that remap will look like on your external drive. Since we know that files on external drives have to be stored
in the Music/ folder, we automatically use that for you. **VERIFY THAT THIS LOOKS LEGIT**. It should look something like this:
Music/SomeArtist - SomeAlbum/SomeTrack.mp3

Your case will differ, especially if you have nested directories, but the whole point of this step is to strip out
the prefix from the source folder to remap it to the destination folder. I hope that isn't confusing, I don't know how
else to explain it.

The program then starts copying all your files. **IF IT ENCOUNTERS A NEW PREFIX THAT IT HASN'T SEEN YET, you will go through
the previous prompt again**. If you're like me, you don't just have 1 folder you placed all your music files in, you have
multiple. So in my case, I was prompted twice to change my prefixes.

This step takes a while and depends on the speed of the drive and the interface it's plugged into. You'll be shown
the progress with a counter so you have an idea of how far along you are.

If there's an error at this stage, we abort and return to the main menu. Yes, the Music directory on that external drive now 
contains only partial data

			IF YOU ENCOUNTER ANY ERRORS, JUST RE-RUN THE EXPORT, AS IT OVERWRITES EVERYTHING YOU'LL BE
			GUARANTEED TO EVENTUALLY GET EVERYTHING COPIED SAFELY.

Next, we copy over the m.db and p.db files to your external drive. If there's an error, you're informed and dumped
back to the main menu. 

Finally, we remap all the paths (remember that step, long ago before the copying took place?) in the DESTINATION
databases so that your turntables (and the Engine Prime software) will be able to correctly find where the tracks are located.

## Option 1.2 Guide For import entire Database:
	
This will copy **just the databases from a source location to your PC's Engine Library folder. It doesn't copy any music**. It 
will prompt you to fix file paths and it will also verify that these files exist so you are prevented from entering invalid values
and screwing up your db. In theory. I'd still make backups. This is useful to totally mirror what's on your external drive to
your PC's Engine Prime library.

VERY IMPORTANT: **This expects that there already exists the files "m.db" and "p.db" in your Engine Library folder.
If you are missing them, simply run the Engine Prime software once and exit: it will create placeholder files for you.**

The app will attempt to auto locate your PC's Engine Library. If it fails or if it guessed wrong, you 
can change this. It will then verify that "m.db" and "p.db" exist there. It keeps prompting you if you keep entering
invalid values, so you can't possibly mess this part up.

Next you're prompted for the source drive letter. This is **WHERE YOU ARE COPYING FROM**. If your external drive was say
F:, you'd enter any of the following: f, f:, f:\\
The app will then verify that this is valid and that AT THE ROOT LEVEL it can find Engine Library\m.db and p.db.
**This was exclusively designed to import from external drives, so it most likely won't help you to import from any other source like
say a network share or different directory on your computer without some finagling on your part.**

Next, the app iterates over every single track in the database. It first asks you to enter the new prefix we should
use for the destination paths for the local files. As the app says, this should be relative to where your PC's m.db
file is located. 

	For example, if we're copying to D:\Music\Engine Library\ (m.db is in here) and our music is located at
	D:\Music\mp3s 
	you'd enter: 
	../mp3s
	
	**********YOU MUST USE / NOT \ AS THE ENGINE PRIME SOFTWARE EXPECTS PATHS IN THIS FORMAT.********** 
	
	If you don't understand how relative paths work or how to figure them out, Google it. 

You will then be shown what the remap will look like and asked to accept. If you say no, you go back into the loop to try again.

DON'T WORRY IF YOU MESS THIS UP: What happens is that for EVERY SINGLE TRACK we check to see if that track exists on your hard drive.
If there's an error, it will print out where it tried to find the file at (to help you diagnose what you entered wrong) and then
you go back to that loop where you enter in the relative prefix to remap.

**This process may happen more than once if, like me, you have tracks stored in different folders. So don't worry if your collection
is scattered, like mine.**

After that, you'll see what can be a slow process where it outputs the current track it's updating and the total number of tracks to
update. I could probably make this part faster by doing some sort of database batching, but honestly, I find database programming to
be so mind numbingly boring that I'm not that interested. If this bothers you, feel free to edit the code and submit a pull request.
I know enough database knowledge to get things done, just not in the most optimal way.


# 2. Import/Export playlists THIS IS ONLY COMPATIBLE WITH DATABASES THAT HAVE BEEN FULLY IMPORTED OR EXPORTED VIA THE ABOVE!!!!!
**I cannot stress this enough: This will only work if you have your databases on the source and destination locations in sync
by having gone through the previous Import/Export entire database section.** The reason is because of how Engine Prime handles
mapping unique track IDs within the database. It can sometimes diverge between the multiple databases for some reason (usually 
when you add tracks from a different root folder, or if you delete then add files) and this makes it nearly impossible for me
to otherwise tell what tracks are the same but just have different paths. **SO ONCE AGAIN, THIS ONLY WORKS IF YOU DID
AN IMPORT/EXPORT OF YOUR ENTIRE DATABASE FROM THE FIRST OPTION**.

Use this to **completely overwrite playlists** on either your computer or an external drive. I suppose you could just drag
and drop them within the EP app, but I've sometimes had problems with this myself. Plus this is faster.

I will now cover the 2 options:

1. EXPORT LOCAL playlists to EXTERNAL DRIVE
2. IMPORT playlists FROM EXTERNAL DRIVE

## Option 2.1 Guide For Exporting Playlists To External Drive
**I cannot stress this enough: This will only work if you have your databases on the source and destination locations in sync
by having gone through the previous Import/Export entire database section.**

	PLEASE RE-READ WHAT I JUST WROTE AS IT'S OF PARAMOUNT IMPORTANCE:
	
	This will only work if you have your databases on the source and destination locations in sync
	by having gone through the previous Import/Export entire database section

The program will attempt to find where you database files are on your PC. If this fails, OR if you decide for whatever reason
I am not aware of due to my use case that you want to specify a different path, you have the option to do so. The program then
tries to find the database files at the path to specify: if it can't find them, you stay in a loop entering the path. This way
you can't mistype the wrong location. 

Next you're prompted for the DESTINATION drive letter. This is **WHERE YOU ARE COPYING TO**. If your external drive was say
F:, you'd enter any of the following: f, f:, f:\\
The app will then verify that this is valid and that AT THE ROOT LEVEL it can find Engine Library\m.db and p.db.
I suppose this could be a mapped network drive, or another drive altogether. But I haven't tested that behavior and won't. It
was designed to export to USB or other external drive.

You're then reminded about the stuff I wrote in bold and you get a prompt asking if you want to return to the main menu in case
you don't want to proceed. If you continue, you'll see some red text that says:

"Press enter to wipe all playlists from destination database."

Press Enter and boom, all your playlists get transferred over from the external drive to your PC, should be very fast. Then you return to the main menu.
**No, this does not copy music files, it just simply mirrors your playlists**. The playlist structure actually (thank god) references track ID numbers rather
than actual file paths so that's why it's so fast **and that's why the track ID matching is so important and why you have to do a full import/export
via this tool first to make sure that the IDs are correctly synced.**

## Option 2.2 Guide For Importing Playlists From External Drive
**I cannot stress this enough: This will only work if you have your databases on the source and destination locations in sync
by having gone through the previous Import/Export entire database section.**

The program will attempt to find where you database files are on your PC. If this fails, OR if you decide for whatever reason
I am not aware of due to my use case that you want to specify a different path, you have the option to do so. The program then
tries to find the database files at the path to specify: if it can't find them, you stay in a loop entering the path. This way
you can't mistype the wrong location. 

Next you're prompted for the source drive letter. This is **WHERE YOU ARE COPYING FROM**. If your external drive was say
F:, you'd enter any of the following: f, f:, f:\\
The app will then verify that this is valid and that AT THE ROOT LEVEL it can find Engine Library\m.db and p.db.
**This was exclusively designed to import from external drives, so it most likely won't help you to import from any other source like
say a network share or different directory on your computer without some finagling on your part.**

You're then reminded about the stuff I wrote in bold and you get a prompt asking if you want to return to the main menu in case
you don't want to proceed. If you continue, you'll see some red text that says:

"Press enter to wipe all playlists from destination database."

Press Enter and boom, all your playlists get transferred over from the external drive to your PC, should be very fast. Then you return to the main menu.
**No, this does not copy music files, it just simply mirrors your playlists** The playlist structure actually (thank god) references track ID numbers rather
than actual file paths so that's why it's so fast **and that's why the track ID matching is so important and why you have to do a full import/export
via this tool first to make sure that the IDs are correctly synced.**

# 3 Import/Export Crates
**THIS FUNCTIONS EXACTLY THE SAME AS Import/Export Playlists WITH THE EXACT SAME REQUIREMENTS AND CAVEATS. PLEASE READ SECTION NUMBER 2 
FOR A DETAILED EXPLANATION OF THIS FEATURE.** The only thing this does differently is it just imports or exports CRATES instead of playlists
so there's no reason to go into a deep dive on the usage. It is literally the same thing as the playlist stuff, just with crates.

# 4 Import/Export Metadata (Cues/Loops/Waveforms/Etc)
**I cannot stress this enough: This will only work if you have your databases on the source and destination locations in sync
by having gone through the previous Import/Export entire database section.**

This lets you selectively copy over cue and loop points (along w/ all their other data like names and colors you picked), as well as
other less common metadata like the track waveforms (which come from EP's analysis step when you import a track) and beat related data.
The following 2 options are presented to you:

1. EXPORT LOCAL metadata to EXTERNAL DRIVE
2. IMPORT metadata FROM EXTERNAL DRIVE

**The functionality of importing and exporting is essentially the same, with all the same caveats and requirements. Thus I will
only cover the importing process.**

Once you choose either of the above options, the following 4 import/export options are displayed to you:

	Which metadata do you wish to import (or export if you chose option 1)?
	1. Just cue points
	2. Just loop info
	3. Both cues AND loops
	4. Everything (includes beat data, waveform analysis, cues, loops, etc)
	5. Back
	
	

## 4.2.1 Importing Cue Points
The program will attempt to find where you database files are on your PC. If this fails, OR if you decide for whatever reason
I am not aware of due to my use case that you want to specify a different path, you have the option to do so. The program then
tries to find the database files at the path to specify: if it can't find them, you stay in a loop entering the path. This way
you can't mistype the wrong location. If you don't have these files, just simply run EP once then quit: it will make empty ones.

Next you're prompted for the source drive letter (or destination if exporting). This is **WHERE YOU ARE COPYING FROM/TO**. If your external drive was say
F:, you'd enter any of the following: f, f:, f:\\
The app will then verify that this is valid and that AT THE ROOT LEVEL it can find Engine Library\m.db and p.db.
**This was exclusively designed to import from external drives, so it most likely won't help you to import from any other source like
say a network share or different directory on your computer without some finagling on your part.**

After this you'll see in magenta text something that looks like this:

	Updating 616 tracks. This may take anywhere from a few seconds to minutes depending on size.
	
I have a small number of tracks so for me this takes 3 seconds. If you have a shit load, it may take longer. What happens during this step
is we read all the performance data out of the performance database **for each track** and copy over all the cue point info **for each track**.

Once that's done you go back to the main menu. No other steps.

## 4.2.2 Importing Loop Points
This functions **exactly like 4.2.1: Importing Cue Points** so please read that section. The only difference is this **only** copies loop
points, not cue points.

## 4.2.3 Importing Both Cues AND Loops
This is legit the exact same thing as steps 4.2.1 and 4.2.2 combined together. Just read section 4.2.1, it's the same thing it just copies
both types of data.

## 4.2.4 Importing Everything 
This straight up copies **every piece of performance data for each track**. What does that mean, you ask? Looking in p.db, the performance database,
that means that besides copying over your loop and cue point info, it will also copy over the precomputed waveform data (there's 2 of these, one
looks like it's a high res version of the wave form, another looks like an overview/preview/simpler version of the wave form), and
any beat data (beat grid and beat markers, there's 2: one analyzed by EP/turntables and an _adjusted_ one if you modified it by hand).

**This process functions exactly the same as all the other options (obviously except what it copies), so please read 4.2.1 for details**. 

# FAQ
	1. How come some characters show up strangely? 
	
	I can't seem to figure out how to get the console to properly display Unicode file/directory names. I've
	set it to use Unicode but that didn't help. You can try right-clicking the title bar and changing the font 
	to see if maybe another font works better. For example, I for some reason have directories that use
	what's called an "em-dash". On Windows, if you type "--" it auto changes it to a fancy looking special dash 
	character. In my case, this will show up in the console as "?-", notice the ?. You may see a box like □ or 
	similar. DON'T FRET: The code correctly stores these paths (I've verified), this is just simply a visual
	display issue and does NOT affect in any way the safety of copying your music files.
			
	2. Can you add feature X?
	
	Most likely: No. If it's not something that I'd care to use, I probably won't add it. This was
	a quick and dirty side project for me that I decided to share with everyone. I have other things
	I need to be doing during my day and as this is free and I'm not being paid for it, I'd rather
	do the other things that pay me. Or play video games. Or just do something else. If you program,
	you're welcome to implement the feature and submit a pull request and I'll review it and add it.
	
	3. I found a bug!
	
	DO NOT REPORT BUGS ON THE DENON FORUM AS I HAVE NOTIFICATIONS DISABLED SINCE THE INTERNET IS A CESSPOOL
	OF WHINING DBAGS WHO COMPLAIN EVEN WITH FREE STUFF. If you found a bug, please file an issue here on
	GitHub. Look at the very top of this page: you'll see "Issues" to the right of "Code" and next to 
	"Pull requests". These I will be notified of and can comment on and keep track of. If you post bugs 
	to the forum, I'll never be able to have an organized way of dealing with issues.
	
	4. Can you add a feature to remap file paths?
	
	No. Another user has already provided a tool to do this. Also, as I said, I wrote this tool for myself
	and my needs. The one time I had to manually change/fix my paths I just opened up m.db and with a very
	small/simple SQLite piece of syntax updated and fixed everything. If this is beyond your scope of 
	comfortability/knowledge, please look into that tool I mentioned (can't recall who posted it or where
	it's located, try searching the forums) or try and see if you can figure out the query syntax. You can
	use a free database editing program like [SQLite Browser](https://sqlitebrowser.org/) which works on Windows and Mac.
	Open up your m.db and browse the "Track" database inside it. You'll see a column named "Path" that has
	the relative location of all your tracks. This is what you'd want to modify.
	
	5. It didn't work when I did anything from options 2 through 4, or worse it caused errors!
	
	As stated in bold letters at the start of each section, these options ONLY work if you imported or 
	exported the database you're trying to use as the source using my tool via option 1, the wholesale import/
	export of the database which includes the music and the path remapping. If you don't do this, it COULD
	work, BUT the problem is that EP can sometimes have unique track IDs that are mismatched and that makes
	it impossible for me to guarantee any reliability or validity of the data copied. That's why it's so
	important that you do a full export/import using my tool at least once.
	
	6. What about a sync feature?
	
	I thought about it. Then I realized it would take more time than I have to work on. The main obstacle is
	that I (and apparently all sources I found on the internet) don't quite know how to interpret what is called
	the "changelog" table in each of the databases. I've tried some tests, and sometimes I get it to add a
	changelog entry for a track, but some actions don't cause an entry. So I don't know a good non-brute force
	approach to doing a standard sync operation. Honestly, I think the best workflow is to commit to either
	working solely in EP or solely on the turntables and then importing/exporting as needed. I think trying
	to do multiple things in multiple places is just going to complicate your life.
	
	7. Can you release a Mac or Linux build?
	
	Linux, definitely not. I don't have access to a Linux box or have the time to set up a development 
	environment on one. I also hate Linux and will never have a use case for this app on Linux. I DO
	have a Mac, but I'm not too familiar with Macs or how Engine Prime works on it, how special folders
	and paths work there, etc. HOWEVER, the code IS written in .Net 5.0 standard which means, that in 
	theory, it's platform independent. Some enterprising individual could just simply take my code 
	and try to recompile a Mac binary. I'd be happy to add it to the release list if that's the case.
	If you are said enterprising individual, please make sure you thoroughly test EVERY feature of this
	program to ensure it works.
		

# Code notes
This code is written quickly to maximize usefulness for me and minimize time spent working on it. It's
not a labor of love, it's a tool to do what I need. In that vein, you will see code that could be better
optimized. NONE OF THAT OPTIMIZATION MATTERS however, as it has negligible real world actual performance
impact on the running time of the application. The only thing that takes time is the file copying process
to external drives, something I can't speed up anyway. SO I DON'T WANT TO HEAR ANY BULLSHIT ABOUT OH
THIS COULD BE DONE BETTER, it's written for legibility and for quick implementation, AND FOR MY NEEDS.

# Contributing
You're welcome to contribute additions/improvements. I'm not interested in performance updates, unless
you can prove to me with empirical data via actual performance analysis. that a perceptible difference exists in the changes.
