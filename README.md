# EnginePrimeSync
This is a command line tool with various utilities to help you manage your Denon Engine Prime database.
I wrote this because I was frustrated with various shortcomings of the GUI app (as of this date, 8/19/21)
such as the inability to import playlists/crates from external drives, inability to wholesale import
an external database to overwrite your local copy (useful if you're restoring a backup or just want
everything to match what you did on the devices), ability to remap paths where files are stored in case
you move your db location or the files, etc.

# REQUIREMENTS
If you're exporting to an external drive, THE DRIVE MUST BE FORMATTED AS exFat! This is because exFat
supports Unicode characters in file and directory names. FAT32 does not. Also there's just no reason
to use FAT32 anyway, every operating system can read exFat.

# COMPATIBILITY
This is designed to only work with edits done via the native Engine Prime app and the SC5000/6000
turntables themselves (i.e. working on the device itself). I do not use nor care about Traktor,
iTunes, Serrato, etc. Thus, this tool will only operate on the main database files. These files are:

m.db - aka "Main DB", holds most of your track and path information
p.db - aka "Performance DB" holds things like cues/loops, waveform analysis, etc.

DON'T USE THIS IF YOU USE OTHER SOFTWARE TO MANAGE YOUR LIBRARIES.

# WARNING READ THIS IF YOU READ NOTHING ELSE
This tool is not guaranteed to be 100% bug free. I think it is, because I've tested it on my own libraries.
But you may have a setup that I didn't expect or plan for (I wrote this on Windows, haven't tested on other platforms
even though the code is platform independent). It also overwrites existing data, that being databases, and in 
the case of a wholesale export to an external drive, the music that was previously on the external drive.

# TL;DR MAKE A BACKUP OF YOUR LIBRARY FILES
Specifically what you want to back up are "m.db" and "p.db" located wherever your "Engine Library"
folder is. This will generally be in your music directory (at least on Windows), and will be in the
root directory on external drives. FOR EXTERNAL DRIVES YOU MAY ALSO WANT TO BACK UP THE ENTIRE
MUSIC SUBDIRECTORY. This folder contains your remapped music files.

# Exporting DB Notes
I create the Music directory structure differently on the external drive than the Engine Prime software
does. Let's say on your PC you have your music files stored at: 

D:\MyMusic\Artist - Album Name\Track1.flac etc.

Engine Prime will create the directory in the following way:

Music\Album Artist (may differ from artist)\Album Name\Track1.flac etc

FURTHERMORE, because it writes assuming the lowest common denominator (FAT32), it strips all Unicode and
special characters. Periods become underscores, umlauts become something random, etc.

This app straight up mirrors your original directory structure to the external drive. Unicode file names and all.
So with the above example, it'd look like this with my app:

Music\Artist - Album Name\Track1.flac etc
Exactly how you had it stored on your drive.

# USAGE GUIDE
I've tried to make the app display the info intelligently when you use it, and I've tried to make it
fault tolerant, i.e. if you enter the wrong values it doesn't let you proceed and just asks for input
again. BACK UP YOUR SHIT BEFORE YOU PROCEED. Anyway, let's go through each option:

# 1. Import/Export entire Database
Use this to WHOLESALE copy an ENTIRE database either from your computer to an external drive,
or IMPORT COMPLETELY a database from an external drive. Let's go through both options:

1. EXPORT LOCAL db to EXTERNAL DRIVE (will copy music files too)
2. IMPORT database FROM EXTERNAL DRIVE (doesn't copy music)

## Option 1 Guide For Import/Export entire Database:
This will completely wipe your external hard drive's Music folder and the m.db and p.db files located there. This option
is meant to do a total mirror of what you have on your PC. Upon choosing this, it attemps to auto find where your database
files are stored and will show you this path if it does. If it doesn't, you are prompted to enter a path. Regardless,
you have the option to change the path from the default. **Enter The FULL path to the folder containing m.db/p.db. Trailing
slash doesn't matter in this case**. For example, if your m.db file is located at D:\DropBox\Music\Engine Library\m.db you'd
enter: D:\DropBox\Music\Engine Library\

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

../Archived Vinyl/<track directory and filename>

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

## Option 2 Guide For Import/Export entire Database:
	
This will copy just the databases from a source location to your PC's Engine Library folder. **It doesn't copy any music**. It 
will prompt you to fix file paths and it will also verify that these files exist so you are prevented from entering invalid values
and screwing up your db. In theory. I'd still make backups.

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
D:\Music\mp3s, you'd enter: ../mp3s
YOU MUST USE / NOT \ AS THE ENGINE PRIME SOFTWARE EXPECTS PATHS IN THIS FORMAT. If you don't understand how relative paths
work or how to figure them out, Google it. 

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



# FAQ
		1. How come some characters show up strangely? 
			I can't seem to figure out how to get the console to properly display Unicode file/directory names. I've set it
			to use Unicode but that didn't help. You can try right-clicking the title bar and changing the font to see if
			there's maybe a font that works better (you could also make the font smaller or bigger too). This font change
			persists through all sessions of the app. For exmaple, I for some reason have directories that use
			what's called an "em-dash". On Windows, if you type "--" it auto changes it to a fancy looking special dash 
			character. In my case, this will show up in the console as "?-", notice the ?. You may see a box or something
			similar. DON'T FRET: The code correctly stores these paths (I've verified), this is just simply a visual
			display issue and does NOT affect in any way the safety of copying your music files.
			
		2. Can you add feature X?
			Most likely: No. If it's not something that I'd care to use, I probably won't add it. This was
			a quick and dirty side project for me that I decided to share with everyone. I have other things
			I need to be doing during my day and as this is free and I'm not being paid for it, I'd rather
			do the other things that pay me. Or play video games. Or just do something else. If you program,
			you're welcome to implement the feature and submit a pull request and I'll review it and add it.

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
