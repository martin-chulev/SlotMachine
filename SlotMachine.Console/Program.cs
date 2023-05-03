using Newtonsoft.Json;
using SlotMachine.Core;
using SlotMachine.Core.Config;

const string GAME_SETTINGS_PATH = "Config/GameSettings.json";
const string TEXTS_PATH = "Config/Texts.json";

// TODO: Validate configuration
var gameSettings = File.Exists(GAME_SETTINGS_PATH) ? JsonConvert.DeserializeObject<GameSettings>(File.ReadAllText(GAME_SETTINGS_PATH)) : new GameSettings();
var texts = File.Exists(TEXTS_PATH) ? JsonConvert.DeserializeObject<Texts>(File.ReadAllText(TEXTS_PATH)) : new Texts();

File.WriteAllText("GameSettings.json", JsonConvert.SerializeObject(gameSettings, Formatting.Indented));
File.WriteAllText("Texts.json", JsonConvert.SerializeObject(texts, Formatting.Indented));

var slotMachine = new SimplifiedSlotMachine(
    (text) => Console.WriteLine(text),
    () => Console.ReadLine(),
    texts ?? new(),
    gameSettings ?? new()
);

slotMachine.Start();