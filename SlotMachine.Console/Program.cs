using SlotMachine.Core;

var slotMachine = new SimplifiedSlotMachine((text) => Console.WriteLine(text), () => Console.ReadLine());
slotMachine.Start();