"use strict";
var TickRate = 16;
var DurationLobby = 4;
var DurationRoundResults = 5;
var DurationBattleEnding = 3;
var NecessaryWins = 3;
var MaxPlayers = 2;
var PlayerNotFound = -1;
var CollectionUser = "User";
var KeyTrophies = "Trophies";
var ScoreFirstPlayer = 0;
var ScoreSecendPlayer = 0;
var IdLeaderboard = "b7c182b36521Win";
var Mode = "ThreeByThree";
/* A dictionary of functions. */
var MessagesLogic = {
    7: ChooseTurnPlayer,
    8: Rematch,
    10: StickersManager
};
