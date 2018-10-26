
namespace com.perroelectrico.flip.util {

    class Tracking {
        public static readonly string HINT_BUTTON = "game:hint:button";
        public static readonly string UNDO_BUTTON = "game:undo:button";
        public static readonly string NEXT_PUZZLE_BUTTON = "game:next_puzzle:button";

        public static readonly string OPTIONS_LANGUAGE = "options:language";

        public static readonly string MENU_GOTO = "menu:goto";

        public static readonly string START_GOTO_TYPE = "start:goto_type";

        public static readonly string GAME_FLIP = "game:flip";
        public static readonly string GAME_UNDO_FLIP = "game:undo:flip";
        public static readonly string GAME_TRY_AGAIN = "game:try_again";
        public static readonly string GAME_NEXT_PUZZLE = "game:next_puzzle";
        public static readonly string GAME_EXIT = "game:exit:menu";
        public static readonly string GAME_SET_PUZZLE = "game:set_puzzle";
        public static readonly string GAME_SET_MUSIC = "game:set_music";
        public static readonly string GAME_SET_SOUNDS = "game:set_sounds";
        public static readonly string GAME_PUZZLE_SOLVED = "game:solved";

        public static readonly string LEVEL_TUTORIAL_DONE = "level:tutorial_done";
        public static readonly string LEVEL_TUTORIAL_START = "level:tutorial_start";

        public static readonly string LEVEL_MASTERED = "level:mastered";
        public static readonly string LEVEL_FINISHED = "level:finished";
        public static readonly string BADGE_EARNED = "badge:earned";

        public static readonly string ERROR_NO_PUZZLES_AVAILABLE = "error:no_puzzles";

        public static void Track(string eventName) { }

        public static void Track(string eventName, int value) { }
    }
}