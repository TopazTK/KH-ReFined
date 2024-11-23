
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using ReFined.Common;
using ReFined.KH2.Information;
using ReFined.Libraries;

namespace ReFined.KH2.Menus
{
    public class Continue
    {
        public class Entry
        {
            public ushort Opcode;
            public ushort Label;

            public ushort[] Export()
            {
                var _returnList = new List<ushort>()
                {
                    Opcode,
                    Label,
                };

                return _returnList.ToArray();
            }
        }

        public ObservableCollection<Entry> Children;

        public Continue()
        {
            Terminal.Log("Initializing Menu: Continue, with Default Parameters...", 0);

            var _entContinue = new Entry()
            {
                Opcode = 0x0002,
                Label = 0x8AB0
            };
            
            var _entLoad = new Entry()
            {
                Opcode = 0x0001,
                Label = 0x8AAF
            };

            Children = new ObservableCollection<Entry>()
            {
                _entContinue,
                _entLoad
            };

            Children.CollectionChanged += Submit;

            Terminal.Log("Menu initialized!", 0);

            Submit();
        }
        public void Submit(object? sender = null, NotifyCollectionChangedEventArgs e = null)
        {
            var _continueOptions = Hypervisor.Read<ulong>(Variables.PINT_GameOverOptions);

            if (sender != null)
                Terminal.Log("Editing Entries of Menu: Continue...", 0);

            else
                Terminal.Log("Submitting Menu: Continue - " + Children.Count + " Entries detected!", 0);

            Hypervisor.Write(_continueOptions + 0x34A, (short)Children.Count, true);

            if (Children.Count > 4)
            {
                Terminal.Log("Error whilst Submitting Menu: Continue - More than 4 entries detected!", 2);
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                if (i < Children.Count)
                {
                    var _childExport = Children[i].Export();
                    var _childWrite = _childExport.SelectMany(BitConverter.GetBytes).ToArray();
                    Hypervisor.Write(_continueOptions + 0x34C + (ulong)(0x04 * i), _childWrite, true);
                }

                else
                    Hypervisor.Write(_continueOptions + 0x34C + (ulong)(0x04 * i), 0x00, true);

            }
         
            if (sender == null)
                Terminal.Log("Menu has been submitted successfully!", 0);
        }
    }
}