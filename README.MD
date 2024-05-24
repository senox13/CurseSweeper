# **CurseSweeper**

A simple CLI implementation of the classic game [MineSweeper](https://en.wikipedia.org/wiki/Minesweeper_(video_game)) in C#, using the libraries [Sharpie](https://github.com/pavkam/sharpie) and [System.CommandLine](https://www.nuget.org/packages/System.CommandLine). This project existed almost entirely as a way for me to learn both libraries, but the code is now clean enough that I'm sharing it publicly.

A special thank you to Alexandru Ciobanu, the dev behind Sharpie, for personally helping me get Sharpie initially set up when I reached out for support.

## Controls

| Button | Action       |
|--------|--------------|
| Space  | uncover tile |
| Enter  | flag mine    |
| r      | reset game   |
| ctrl-c | exit game    |

## CLI options
```
  -d, --difficulty <difficulty>  Specifies a preset difficulty. Must be one of the following: beginner, intermediate, expert
  -w, --width <width>            Specifies a custom width for the game board
  -h, --height <height>          Specifies a custom height for the game board
  -m, --mines <mines>            Specifies a custom number of mines to place on the game board
  --version                      Show version information
  -?, -h, --help                 Show help and usage information
```

## License

The full contents of this repository are licensed under the GNU GPLv3.

## Contact

I can be reached at, in descending order of preference:
* The issue tracker on [this repo](https://github.com/senox13/CurseSweeper)
* Discord: `senox13`
* Email: `senox13@gmail.com`
