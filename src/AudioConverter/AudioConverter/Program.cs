/*
 TODO:
 1. Detect if input file contains any DTS streams
 2a. If so, prompt user to select which DTS streams to convert (multiple selection)
 2b. If not, prompt user that there are no DTS streams to convert.
 3. Detect if input file has any subtitles
 3a. If so, detect if if any are in English or Danish (configurable?)
 3b. If so, keep the wanted subtitles in the output file
 3b. If not, prompt user that the file contains no embedded subtitles.
 4. If neither 1 or 3 is true, prompt user that the file contains no DTS streams or subtitles and exit.
 */

var app = new CommandApp<ConvertCommand>();
return await app.RunAsync(args);
