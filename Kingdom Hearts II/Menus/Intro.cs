
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using ReFined.Common;
using ReFined.KH2.Information;
using ReFined.Libraries;

namespace ReFined.KH2.Menus
{
    public class Intro
    {
        public class Entry
        {
            public uint Count;
            public uint Flair;
            public uint Title;
            public List<uint> Buttons;
            public List<uint> Descriptions;

            public Entry(uint Count, uint Flair, uint Title, uint[] Buttons, uint[] Descriptions)
            {
                this.Count = Count;
                this.Flair = Flair;
                this.Title = Title;
                this.Buttons = new List<uint>();
                this.Descriptions = new List<uint>();

                this.Buttons.AddRange(Buttons);
                this.Descriptions.AddRange(Descriptions);
            }

            public uint[] Export()
            {
                var _returnList = new List<uint>()
                {
                    Count,
                    Flair,
                    Title 
                };

                for (int i = 0; i < 4; i++)
                {
                    if (i < Count)
                        _returnList.Add(Buttons[i]);

                    else
                        _returnList.Add(0xFFFFFFFF);
                }

                for (int i = 0; i < 4; i++)
                {
                    if (i < Count)
                        _returnList.Add(Descriptions[i]);

                    else
                        _returnList.Add(0xFFFFFFFF);
                }

                return _returnList.ToArray();
            }
        }

        public ObservableCollection<Entry> Children;

        public Intro()
        {
            Terminal.Log("Initializing Menu: Intro, with Default Parameters...", 0);

            var _entDifficulty = new Entry(4, 0xC330, 0xC380, [0xC331, 0xC332, 0xC333, 0xCE33], [0xC334, 0xC335, 0xC336, 0xCE34]);
            var _entVibration = new Entry(2, 0xC337, 0xC381, [0xC338, 0xC339], [0xC33A, 0xC33B]);
            var _entAutosave = new Entry(3, 0x0133, 0x0000, [0x0105, 0x0107, 0x0109], [0x0106, 0x0108, 0x010A]);
            var _entController = new Entry(2, 0x0137, 0x0000, [0x0123, 0x0125], [0x0124, 0x0126]);

            Children = new ObservableCollection<Entry>()
            {
                _entDifficulty,
                _entVibration,
                _entAutosave,
                _entController,
            };

            Children.CollectionChanged += Submit;

            Terminal.Log("Menu initialized!", 0);

            Submit();
        }

        public void Submit(object? sender = null, NotifyCollectionChangedEventArgs e = null)
        {
            if (sender != null)
                Terminal.Log("Inserting New Entry to Intro...", 0);

            else
                Terminal.Log("Submitting Menu: Intro - " + Children.Count + " Entries detected!", 0);

            for (int i = 0; i < Children.Count; i++)
            {
                var _childExport = Children[i].Export();
                var _childWrite = _childExport.SelectMany(BitConverter.GetBytes).ToArray();

                Hypervisor.Write(Variables.ADDR_NewGameMenu + (ulong)(i * 0x2C), _childWrite);
            }
            
            byte _lastIndex = (byte)(Children.Count - 1);

            // Redirect the menu table.
            
            Hypervisor.Write(Variables.HFIX_IntroOffsets[0] + 0x233, 0x820204);
            Hypervisor.Write(Variables.HFIX_IntroOffsets[0] + 0x253, 0x820200);
            Hypervisor.Write(Variables.HFIX_IntroOffsets[0] + 0x276, 0x82020C);
            Hypervisor.Write(Variables.HFIX_IntroOffsets[0] + 0x406, 0x82021C);

            Hypervisor.RedirectLEA(Variables.HFIX_IntroOffsets[1] + 0x0AF, 0x820208);
            Hypervisor.RedirectLEA(Variables.HFIX_IntroOffsets[1] + 0x1DA, 0x820200);
            Hypervisor.RedirectLEA(Variables.HFIX_IntroOffsets[1] + 0x3D7, 0x820200);
            Hypervisor.RedirectLEA(Variables.HFIX_IntroOffsets[2] + 0x03D, 0x82021C);

            // Write the counts and the last indexes.

            Hypervisor.Write(Variables.HFIX_IntroOffsets[3] + 0x097, (byte)Children.Count);
            Hypervisor.Write(Variables.HFIX_IntroOffsets[3] + 0x1F5, (byte)Children.Count);
            Hypervisor.Write(Variables.HFIX_IntroOffsets[3] + 0x531, (byte)Children.Count);
            Hypervisor.Write(Variables.HFIX_IntroOffsets[4] + 0x1EF, (byte)Children.Count);

            Hypervisor.Write(Variables.HFIX_IntroOffsets[0] + 0x3F7, _lastIndex);
            Hypervisor.Write(Variables.HFIX_IntroOffsets[1] + 0x3CB, _lastIndex);
            Hypervisor.Write(Variables.HFIX_IntroOffsets[2] + 0x031, _lastIndex);
            Hypervisor.Write(Variables.HFIX_IntroOffsets[3] + 0x08E, _lastIndex);

            // Redirect the chosen option memory space.

            Hypervisor.Write(Variables.HFIX_IntroOffsets[0] + 0x3B5, 0x820500);

            Hypervisor.RedirectLEA(Variables.HFIX_IntroOffsets[1] + 0x0A8, 0x820500);
            Hypervisor.RedirectLEA(Variables.HFIX_IntroOffsets[4] + 0x1F2, 0x820500);
            Hypervisor.RedirectMOV(Variables.HFIX_IntroOffsets[5] + 0x2BF, 0x820500);

            if (sender == null)
            Terminal.Log("Menu submitted successfully!", 0);
        }
    }
}