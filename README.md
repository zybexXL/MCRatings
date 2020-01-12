
About MCRatings
------

<img align="right" src="https://github.com/zybexXL/MCRatings/blob/master/Screenshots/MCRatings.png">

MCRatings is a companion tool for JRiver Media Center to help tag a movie collection. It retrieves IMDb, TMDb, Metacritic and Rotten Tomatoes movie ratings, along with other movie metadata such as Tagline, Actors, Director, Studios, etc from Open Movie Database (OMDb) and The Movie Database (TMDb). You can decide what changes to keep/skip, and which JRiver fields to update.

[Features](#Features)<br>
[Requirements](#Requirements)<br>
[Quick Start](#Quick-Start)<br>
[User Guide](https://github.com/zybexXL/MCRatings/wiki)<br>
[Screenshots](https://github.com/zybexXL/MCRatings/wiki/Screenshots)<br>
[Download](https://github.com/zybexXL/MCRatings/releases/latest)<br>

*Note: MCRatings was previously called JRatings*

<br>

Features
------
This is what **MCRatings** can do for you:

- Gets movie metadata from OMDb and TMDb including IMDb/TMDb/RottenTomatoes/Metacritic ratings...
- ... Actors, Director, Writers, Tagline, Keywords, Awards, Plot, Website, Trailer... and a few more!
- Uses IMDb ID if available (tt number), or movie Title/Year otherwise.
- Automagically extracts movie title/year from your filenames and folders.
- Uses the JRiver API to load and update movie tags, so there's no need to mess with Sidecar XMLs.
- Mapping of which JRiver fields to use and which ones to allow overwrite
- Easily revert changes to a movie field, movie row, or entire column of data
- Lets you easily review new and changed movie metadata before committing it to Jriver.
- Very functional UI with search/filtering/sort, batch update, click-to-open IMDb page and movie folder, etc.
- Color coding of changed fields and fields with new information; review of new vs previous value
- Easter eggs!

<br>

Requirements
------
- .Net Framework 4.6.2 or above (Windows 10 should have it by default)
- JRiver MediaCenter must be installed on same PC (client or server)
- OMDb API key (free or otherwise) - get it from [here](http://www.omdbapi.com/apikey.aspx)
- TMDb API key (free) - get it from [here](http://www.themoviedb.org/settings/api) (you may need to register a TMDb account first)

<br>

Quick Start
------
1. Backup your JRiver library.
2. Go back to step 1 and really do it this time.  
3. Start MCRatings - at first run time, the [Settings UI](#Configuration) will automatically open
   * Check the MCRatings<->JRiver field mapping. For any fields marked RED you need to:
     * create the missing fields in JRiver (with type = string)
     * disable them if you are not interested in the information they provide.  
   * Register for an [OMDb Key](http://www.omdbapi.com/apikey.aspx) and enter it on "OMDb API Keys"
   * Register for a [TMDb Key](https://developers.themoviedb.org/3/getting-started/introduction) and enter it on "TMDb API Keys"
   * Save Settings
4. Select a JRiver Playlist to load
5. Select a few movies and click "Get Movie Info"
6. Review the info downloaded from TMDb/OMDb and save it to your JRiver DB using "Save to JRiver"
7. Rejoice that you now have a whole lot of metadata per movie in your collection and enjoy it!

