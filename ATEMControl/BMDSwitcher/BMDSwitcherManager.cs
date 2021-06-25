﻿using BMDSwitcherAPI;
using Integrated_Presenter.BMDSwitcher;
using Integrated_Presenter.BMDSwitcher.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Integrated_Presenter
{

    public delegate void SwitcherStateChange(BMDSwitcherState args);

    public class BMDSwitcherManager : IBMDSwitcherManager
    {

        private IBMDSwitcherDiscovery _BMDSwitcherDiscovery;
        private IBMDSwitcher _BMDSwitcher;

        private IBMDSwitcherMixEffectBlock _BMDSwitcherMixEffectBlock1;
        private IBMDSwitcherInputAux _BMDSwitcherAuxInput;
        private IBMDSwitcherKey _BMDSwitcherUpstreamKey1;
        private IBMDSwitcherDownstreamKey _BMDSwitcherDownstreamKey1;
        private IBMDSwitcherDownstreamKey _BMDSwitcherDownstreamKey2;
        private List<IBMDSwitcherInput> _BMDSwitcherInputs = new List<IBMDSwitcherInput>();
        private IBMDSwitcherMultiView _BMDSwitcherMultiView;
        private IBMDSwitcherKeyFlyParameters _BMDSwitcherFlyKeyParamters;
        private IBMDSwitcherTransitionParameters _BMDSwitcherTransitionParameters;

        private SwitcherMonitor _switcherMonitor;
        private MixEffectBlockMonitor _mixEffectBlockMonitor;
        private BMDSwitcherAuxMonitor _auxMonitor;
        private UpstreamKeyMonitor _upstreamKey1Monitor;
        private DownstreamKeyMonitor _dsk1Monitor;
        private DownstreamKeyMonitor _dsk2Monitor;
        private List<InputMonitor> _inputMonitors = new List<InputMonitor>();
        private SwitcherFlyKeyMonitor _flykeyMonitor;
        private SwitcherTransitionMonitor _transitionMontitor;
        // prehaps don't need to monitor multiviewer
        // for now won't have mediaplayer monitors

        private BMDSwitcherState _state;


        public event SwitcherStateChange SwitcherStateChanged;

        public bool GoodConnection { get; set; } = false;
        bool IBMDSwitcherManager.GoodConnection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Window _parent;

        private BMDSwitcherConfigSettings _config;

        public BMDSwitcherManager(Window parent)
        {
            _parent = parent;

            _switcherMonitor = new SwitcherMonitor();
            _switcherMonitor.SwitcherDisconnected += _switcherMonitor_SwitcherDisconnected;


            _mixEffectBlockMonitor = new MixEffectBlockMonitor();
            _mixEffectBlockMonitor.PreviewInputChanged += _mixEffectBlockMonitor_PreviewInputChanged;
            _mixEffectBlockMonitor.ProgramInputChanged += _mixEffectBlockMonitor_ProgramInputChanged;
            _mixEffectBlockMonitor.FateToBlackFullyChanged += _mixEffectBlockMonitor_FateToBlackFullyChanged;
            _mixEffectBlockMonitor.InTransitionChanged += _mixEffectBlockMonitor_InTransitionChanged;

            _auxMonitor = new BMDSwitcherAuxMonitor();
            _auxMonitor.OnAuxInputChanged += _auxMonitor_OnAuxInputChanged;

            _upstreamKey1Monitor = new UpstreamKeyMonitor();
            _upstreamKey1Monitor.UpstreamKeyOnAirChanged += _upstreamKey1Monitor_UpstreamKeyOnAirChanged;
            _upstreamKey1Monitor.UpstreamKeyFillChanged += _upstreamKey1Monitor_UpstreamKeyFillChanged;
            _upstreamKey1Monitor.UpstreamKeyTypeChanged += _upstreamKey1Monitor_UpstreamKeyTypeChanged;
            _upstreamKey1Monitor.UpstreamKeyCutChanged += _upstreamKey1Monitor_UpstreamKeyCutChanged;
            _upstreamKey1Monitor.UpstreamKeyMaskChanged += _upstreamKey1Monitor_UpstreamKeyMaskChanged;

            _flykeyMonitor = new SwitcherFlyKeyMonitor();
            _flykeyMonitor.KeyFrameChanged += _flykeyMonitor_KeyFrameChanged;
            _flykeyMonitor.KeyFrameStateChange += _flykeyMonitor_KeyFrameStateChange;

            _transitionMontitor = new SwitcherTransitionMonitor();
            _transitionMontitor.OnTransitionSelectionChanged += _transitionMontitor_OnNextTransitionSelectionChanged;

            _dsk1Monitor = new DownstreamKeyMonitor();
            _dsk1Monitor.OnAirChanged += _dsk1Manager_OnAirChanged;
            _dsk1Monitor.TieChanged += _dsk1Manager_TieChanged;
            _dsk2Monitor = new DownstreamKeyMonitor();
            _dsk2Monitor.OnAirChanged += _dsk2Manager_OnAirChanged;
            _dsk2Monitor.TieChanged += _dsk2Manager_TieChanged;

            _BMDSwitcherDiscovery = new CBMDSwitcherDiscovery();
            if (_BMDSwitcherDiscovery == null)
            {
                MessageBox.Show("Could not create Switcher Discovery Instance.\nATEM Switcher Software not found/installed", "Error");
            }
            _state = new BMDSwitcherState();
            SwitcherDisconnected();
        }

        private void _mixEffectBlockMonitor_InTransitionChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_TransitionPosition();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _upstreamKey1Monitor_UpstreamKeyMaskChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _upstreamKey1Monitor_UpstreamKeyCutChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _flykeyMonitor_KeyFrameStateChange(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_USK1();
                ForceStateUpdate_PIPSettings();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _auxMonitor_OnAuxInputChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_AuxInput();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _upstreamKey1Monitor_UpstreamKeyTypeChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_USK1();
                ForceStateUpdate_ChromaSettings();
                ForceStateUpdate_PIPSettings();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _upstreamKey1Monitor_UpstreamKeyFillChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _transitionMontitor_OnNextTransitionSelectionChanged(object sender)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_Transition();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _flykeyMonitor_KeyFrameChanged(object sender, int keyframe)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void InputMonitor_ShortNameChanged(object sender, object args)
        {
            //throw new NotImplementedException();
        }

        private void InputMonitor_LongNameChanged(object sender, object args)
        {
            //throw new NotImplementedException();
        }


        private void _upstreamKey1Monitor_UpstreamKeyOnAirChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _mixEffectBlockMonitor_FateToBlackFullyChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_FTB();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk2Manager_TieChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_DSK2();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk2Manager_OnAirChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_DSK2();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk1Manager_TieChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_DSK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk1Manager_OnAirChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_DSK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _mixEffectBlockMonitor_ProgramInputChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_ProgramInput();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _mixEffectBlockMonitor_PreviewInputChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_PreviewInput();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _switcherMonitor_SwitcherDisconnected(object sender, object args)
        {
            SwitcherDisconnected();
        }

        public bool TryConnect(string address)
        {

            _BMDSwitcherConnectToFailure failReason = 0;
            try
            {
                _BMDSwitcherDiscovery.ConnectTo(address, out _BMDSwitcher, out failReason);
                SwitcherConnected();
                return true;
            }
            catch (COMException)
            {
                switch (failReason)
                {
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureNoResponse:
                        MessageBox.Show("No response from Switcher", "Error");
                        break;
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureIncompatibleFirmware:
                        MessageBox.Show("Switcher has incompatible firmware", "Error");
                        break;
                    default:
                        MessageBox.Show("Switcher failed to connect for unknown reason", "Error");
                        break;
                }
                return false;
            }

        }

        private bool InitializeInputSources()
        {
            // get all input sources
            IBMDSwitcherInputIterator inputIterator = null;
            IntPtr inputIteratorPtr;
            Guid inputIteratorIID = typeof(IBMDSwitcherInputIterator).GUID;
            _BMDSwitcher.CreateIterator(ref inputIteratorIID, out inputIteratorPtr);
            if (inputIteratorPtr != null)
            {
                inputIterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(inputIteratorPtr);
            }
            else
            {
                return false;
            }
            if (inputIterator != null)
            {
                IBMDSwitcherInput input;
                inputIterator.Next(out input);
                while (input != null)
                {
                    _BMDSwitcherInputs.Add(input);
                    InputMonitor inputMonitor = new InputMonitor(input);
                    input.AddCallback(inputMonitor);
                    inputMonitor.LongNameChanged += InputMonitor_LongNameChanged;
                    inputMonitor.ShortNameChanged += InputMonitor_ShortNameChanged;
                    _inputMonitors.Add(inputMonitor);
                    inputIterator.Next(out input);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool InitializeAuxInput()
        {
            // get all input sources
            IBMDSwitcherInputIterator inputIterator = null;
            IntPtr inputIteratorPtr;
            Guid inputIteratorIID = typeof(IBMDSwitcherInputIterator).GUID;
            _BMDSwitcher.CreateIterator(ref inputIteratorIID, out inputIteratorPtr);
            if (inputIteratorPtr != null)
            {
                inputIterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(inputIteratorPtr);
            }
            else
            {
                return false;
            }
            if (inputIterator != null)
            {
                IBMDSwitcherInput aux;
                inputIterator.GetById((long)BMDSwitcherVideoSources.Auxillary1, out aux);
                _BMDSwitcherAuxInput = (IBMDSwitcherInputAux)aux;
                _BMDSwitcherAuxInput.AddCallback(_auxMonitor);
                return true;
            }

            return false;
        }

        private bool InitializeMultiView()
        {
            IntPtr multiViewPtr;
            Guid multiViewIID = typeof(IBMDSwitcherMultiViewIterator).GUID;
            _BMDSwitcher.CreateIterator(ref multiViewIID, out multiViewPtr);
            if (multiViewPtr == null)
            {
                return false;
            }
            IBMDSwitcherMultiViewIterator multiViewIterator = (IBMDSwitcherMultiViewIterator)Marshal.GetObjectForIUnknown(multiViewPtr);
            if (multiViewIterator == null)
            {
                return false;
            }

            multiViewIterator.Next(out _BMDSwitcherMultiView);

            return true;
        }


        private bool InitializeMixEffectBlock()
        {
            // get mixeffectblock1
            IBMDSwitcherMixEffectBlockIterator meIterator = null;
            IntPtr meIteratorPtr;
            Guid meIteratorIID = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            _BMDSwitcher.CreateIterator(ref meIteratorIID, out meIteratorPtr);
            if (meIteratorPtr != null)
            {
                meIterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(meIteratorPtr);
            }
            if (meIterator == null)
                return false;


            meIterator.Next(out _BMDSwitcherMixEffectBlock1);

            if (_BMDSwitcherMixEffectBlock1 == null)
            {
                MessageBox.Show("Unexpected: Could not get first mix effect block", "Error");
                return false;
            }

            // add callbacks
            _BMDSwitcherMixEffectBlock1.AddCallback(_mixEffectBlockMonitor);

            // get transitions
            _BMDSwitcherTransitionParameters = (IBMDSwitcherTransitionParameters)_BMDSwitcherMixEffectBlock1;
            _BMDSwitcherTransitionParameters.AddCallback(_transitionMontitor);

            return true;
        }


        private bool InitializeUpstreamKeyers()
        {
            IBMDSwitcherKeyIterator keyIterator;
            IntPtr keyItrPtr;
            Guid keyItrIID = typeof(IBMDSwitcherKeyIterator).GUID;
            _BMDSwitcherMixEffectBlock1.CreateIterator(ref keyItrIID, out keyItrPtr);

            keyIterator = (IBMDSwitcherKeyIterator)Marshal.GetObjectForIUnknown(keyItrPtr);
            keyIterator.Next(out _BMDSwitcherUpstreamKey1);

            _BMDSwitcherUpstreamKey1.AddCallback(_upstreamKey1Monitor);

            _BMDSwitcherFlyKeyParamters = (IBMDSwitcherKeyFlyParameters)_BMDSwitcherUpstreamKey1;
            _BMDSwitcherFlyKeyParamters.AddCallback(_flykeyMonitor);


            return true;
        }

        private bool InitializeDownstreamKeyers()
        {
            // get downstream keyers
            IBMDSwitcherDownstreamKeyIterator dskIterator = null;
            IntPtr dskIteratorPtr;
            Guid dskIteratorIID = typeof(IBMDSwitcherDownstreamKeyIterator).GUID;
            _BMDSwitcher.CreateIterator(ref dskIteratorIID, out dskIteratorPtr);
            if (dskIteratorPtr != null)
            {
                dskIterator = (IBMDSwitcherDownstreamKeyIterator)Marshal.GetObjectForIUnknown(dskIteratorPtr);
            }
            if (dskIterator == null)
                return false;

            if (dskIterator != null)
            {
                // get first dsk
                dskIterator.Next(out _BMDSwitcherDownstreamKey1);
            }
            if (dskIterator != null)
            {
                // get second dsk
                dskIterator.Next(out _BMDSwitcherDownstreamKey2);
            }

            if (_BMDSwitcherDownstreamKey1 == null || _BMDSwitcherDownstreamKey2 == null)
            {
                MessageBox.Show("Unexpected: Could not get one of the downstream keyers", "Error");
                return false;
            }

            // add callbacks
            _BMDSwitcherDownstreamKey1.AddCallback(_dsk1Monitor);
            _BMDSwitcherDownstreamKey2.AddCallback(_dsk2Monitor);

            return true;

        }

      
        private void SwitcherConnected()
        {
            // add callbacks (monitors switcher connection)
            _BMDSwitcher.AddCallback(_switcherMonitor);


            bool mixeffects = InitializeMixEffectBlock();
            bool upstreamkeyers = InitializeUpstreamKeyers();
            bool downstreamkeyers = InitializeDownstreamKeyers();

            bool inputsources = InitializeInputSources();
            bool auxsource = InitializeAuxInput();
            bool multiviewer = InitializeMultiView();


            GoodConnection = mixeffects && auxsource && downstreamkeyers && upstreamkeyers && inputsources && multiviewer;

            MessageBox.Show("Connected to Switcher", "Connection Success");

            // update state
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void SwitcherDisconnected()
        {
            GoodConnection = false;

            _parent.Dispatcher.Invoke(() =>
            {
                // remove callbacks
                if (_BMDSwitcherMixEffectBlock1 != null)
                {
                    _BMDSwitcherMixEffectBlock1.RemoveCallback(_mixEffectBlockMonitor);
                    _BMDSwitcherMixEffectBlock1 = null;
                }

                if (_BMDSwitcherUpstreamKey1 != null)
                {
                    _BMDSwitcherUpstreamKey1.RemoveCallback(_upstreamKey1Monitor);
                    _BMDSwitcherUpstreamKey1 = null;
                }

                if (_BMDSwitcherDownstreamKey1 != null)
                {
                    _BMDSwitcherDownstreamKey1.RemoveCallback(_dsk1Monitor);
                    _BMDSwitcherDownstreamKey1 = null;
                }
                if (_BMDSwitcherDownstreamKey2 != null)
                {
                    _BMDSwitcherDownstreamKey2.RemoveCallback(_dsk2Monitor);
                    _BMDSwitcherDownstreamKey2 = null;
                }

                if (_BMDSwitcherAuxInput != null)
                {
                    _BMDSwitcherAuxInput.RemoveCallback(_auxMonitor);
                    _BMDSwitcherAuxInput = null;
                }

                if (_BMDSwitcher != null)
                {
                    _BMDSwitcher.RemoveCallback(_switcherMonitor);
                    _switcherMonitor = null;
                }

                int i = 0;
                foreach (var input in _BMDSwitcherInputs)
                {
                    input.RemoveCallback(_inputMonitors[i++]);
                }
                _inputMonitors.Clear();
                _BMDSwitcherInputs.Clear();

                if (_BMDSwitcherMultiView != null)
                {
                    // no callback yet
                    _BMDSwitcherMultiView = null;
                }
            });

        }



        public BMDSwitcherState ForceStateUpdate()
        {
            if (GoodConnection)
            {
                // update state
                ForceStateUpdate_ProgramInput();
                ForceStateUpdate_PreviewInput();
                ForceStateUpdate_AuxInput();
                ForceStateUpdate_Transition();
                ForceStateUpdate_USK1();
                ForceStateUpdate_ChromaSettings();
                ForceStateUpdate_PIPSettings();
                ForceStateUpdate_DSK1();
                ForceStateUpdate_DSK2();
                ForceStateUpdate_FTB();
            }
            return _state;
        }

        private void ForceStateUpdate_AuxInput()
        {
            long source;
            _BMDSwitcherAuxInput.GetInputSource(out source);
            _state.AuxID = source;
        }

        private void ForceStateUpdate_USK1()
        {
            int onair;
            _BMDSwitcherUpstreamKey1.GetOnAir(out onair);
            _state.USK1OnAir = onair != 0;

            _BMDSwitcherKeyType keytype;
            _BMDSwitcherUpstreamKey1.GetType(out keytype);

            switch (keytype)
            {
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma:
                    _state.USK1KeyType = 3;
                    break;
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypeChroma:
                    _state.USK1KeyType = 2;
                    break;
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypePattern:
                    _state.USK1KeyType = 4;
                    break;
                case _BMDSwitcherKeyType.bmdSwitcherKeyTypeDVE:
                    _state.USK1KeyType = 1;
                    break;
                default:
                    _state.USK1KeyType = 0;
                    break;
            }

            _BMDSwitcherFlyKeyFrame frame;
            _BMDSwitcherFlyKeyParamters.IsAtKeyFrames(out frame);

            switch (frame)
            {
                case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameFull:
                    _state.USK1KeyFrame = 0;
                    break;
                case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA:
                    _state.USK1KeyFrame = 1;
                    break;
                case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB:
                    _state.USK1KeyFrame = 2;
                    break;
                default:
                    _state.USK1KeyFrame = -1;
                    break;
            }

            long usk1fill;
            _BMDSwitcherUpstreamKey1.GetInputFill(out usk1fill);
            _state.USK1FillSource = usk1fill;

        }

        private void ForceStateUpdate_DSK1()
        {
            int dsk1onair;
            int dsk1tie;
            _BMDSwitcherDownstreamKey1.GetOnAir(out dsk1onair);
            _BMDSwitcherDownstreamKey1.GetTie(out dsk1tie);

            _state.DSK1OnAir = dsk1onair != 0;
            _state.DSK1Tie = dsk1tie != 0;
        }
        private void ForceStateUpdate_DSK2()
        {
            int dsk2onair;
            int dsk2tie;
            _BMDSwitcherDownstreamKey2.GetOnAir(out dsk2onair);
            _BMDSwitcherDownstreamKey2.GetTie(out dsk2tie);

            _state.DSK2OnAir = dsk2onair != 0;
            _state.DSK2Tie = dsk2tie != 0;
        }
        private void ForceStateUpdate_ProgramInput()
        {
            // get current program source
            long programid;
            _BMDSwitcherMixEffectBlock1.GetProgramInput(out programid);
            _state.ProgramID = programid;
        }

        private void ForceStateUpdate_PreviewInput()
        {
            // get current preset source
            long presetid;
            _BMDSwitcherMixEffectBlock1.GetPreviewInput(out presetid);
            _state.PresetID = presetid;
        }

        private void ForceStateUpdate_TransitionPosition()
        {
            double trans_position;
            uint trans_frames_remaining;
            int intrans;

            _BMDSwitcherMixEffectBlock1.GetTransitionPosition(out trans_position);
            _BMDSwitcherMixEffectBlock1.GetTransitionFramesRemaining(out trans_frames_remaining);
            _BMDSwitcherMixEffectBlock1.GetInTransition(out intrans);

            _state.InTransition = intrans != 0;
            _state.TransitionFramesRemaining = (int)trans_frames_remaining;
            _state.TransitionPosition = trans_position;
        }

        private void ForceStateUpdate_Transition()
        {
            _BMDSwitcherTransitionSelection sel;
            _BMDSwitcherTransitionParameters.GetNextTransitionSelection(out sel);
            int selection = (int)sel;

            _state.TransNextBackground = (selection & (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground) == (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground;
            _state.TransNextKey1 = (selection & (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1) == (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1;

        }

        private void ForceStateUpdate_FTB()
        {
            int ftb;
            _BMDSwitcherMixEffectBlock1.GetFadeToBlackFullyBlack(out ftb);
            _state.FTB = ftb != 0;
        }

        public BMDSwitcherState GetCurrentState()
        {
            return _state;
        }



        public void ConfigureSwitcher(BMDSwitcherConfigSettings config)
        {
            _config = config;
            ConfigureMixEffectBlock();
            ConfigureAux();
            ConfigureCameraSources();
            ConfigureDownstreamKeys();
            ConfigureMultiviewer();
            ConfigureUpstreamKey();
            // disable for now - doesn't work
            //ConfigureMediaPool();
            ConfigureAudioLevels();

            ForceStateUpdate();
        }

        private void ConfigureAux()
        {
            _BMDSwitcherAuxInput.SetInputSource(_config.DefaultAuxSource);
        }

        private void ConfigureMixEffectBlock()
        {
            _BMDSwitcherMixEffectBlock1.SetFadeToBlackRate((uint)_config.MixEffectSettings.FTBRate);

            IBMDSwitcherTransitionMixParameters mixParameters = (IBMDSwitcherTransitionMixParameters)_BMDSwitcherMixEffectBlock1;
            mixParameters.SetRate((uint)_config.MixEffectSettings.Rate);
        }

        private void ConfigureCameraSources()
        {

            // set input source names
            foreach (var inputsource in _BMDSwitcherInputs)
            {
                long sourceid;
                inputsource.GetInputId(out sourceid);

                var map = _config.Routing.Where(r => (long)r.PhysicalInputId == sourceid);
                if (map != null && map.Count() > 0)
                {
                    var source = map.First();
                    inputsource.SetLongName(source.LongName);
                    inputsource.SetShortName(source.ShortName);
                }

            }
        }

        private void ConfigureDownstreamKeys()
        {
            ConfigureDownstreamKey1();
            ConfigureDownstreamKey2();
        }

        private void ConfigureDownstreamKey1()
        {
            _BMDSwitcherDownstreamKey1.SetInputFill(_config.DownstreamKey1Config.InputFill);
            _BMDSwitcherDownstreamKey1.SetInputCut(_config.DownstreamKey1Config.InputCut);
            _BMDSwitcherDownstreamKey1.SetRate((uint)_config.DownstreamKey1Config.Rate);

            _BMDSwitcherDownstreamKey1.SetPreMultiplied(_config.DownstreamKey1Config.IsPremultipled);
            _BMDSwitcherDownstreamKey1.SetClip(_config.DownstreamKey1Config.Clip);
            _BMDSwitcherDownstreamKey1.SetGain(_config.DownstreamKey1Config.Gain);
            _BMDSwitcherDownstreamKey1.SetInverse(_config.DownstreamKey1Config.Invert);
            _BMDSwitcherDownstreamKey1.SetMasked(_config.DownstreamKey1Config.IsMasked);
            _BMDSwitcherDownstreamKey1.SetMaskTop(_config.DownstreamKey1Config.MaskTop);
            _BMDSwitcherDownstreamKey1.SetMaskBottom(_config.DownstreamKey1Config.MaskBottom);
            _BMDSwitcherDownstreamKey1.SetMaskLeft(_config.DownstreamKey1Config.MaskLeft);
            _BMDSwitcherDownstreamKey1.SetMaskRight(_config.DownstreamKey1Config.MaskRight);
        }

        private void ConfigureDownstreamKey2()
        {
            _BMDSwitcherDownstreamKey2.SetInputFill(_config.DownstreamKey2Config.InputFill);
            _BMDSwitcherDownstreamKey2.SetInputCut(_config.DownstreamKey2Config.InputCut);
            _BMDSwitcherDownstreamKey2.SetRate((uint)_config.DownstreamKey2Config.Rate);

            _BMDSwitcherDownstreamKey2.SetPreMultiplied(_config.DownstreamKey2Config.IsPremultipled);
            _BMDSwitcherDownstreamKey2.SetClip(_config.DownstreamKey2Config.Clip);
            _BMDSwitcherDownstreamKey2.SetGain(_config.DownstreamKey2Config.Gain);
            _BMDSwitcherDownstreamKey2.SetInverse(_config.DownstreamKey2Config.Invert);
            _BMDSwitcherDownstreamKey2.SetMasked(_config.DownstreamKey2Config.IsMasked);
            _BMDSwitcherDownstreamKey2.SetMaskTop(_config.DownstreamKey2Config.MaskTop);
            _BMDSwitcherDownstreamKey2.SetMaskBottom(_config.DownstreamKey2Config.MaskBottom);
            _BMDSwitcherDownstreamKey2.SetMaskLeft(_config.DownstreamKey2Config.MaskLeft);
            _BMDSwitcherDownstreamKey2.SetMaskRight(_config.DownstreamKey2Config.MaskRight);

        }

        private void ConfigureMultiviewer()
        {
            _BMDSwitcherMultiView.SetLayout((_BMDSwitcherMultiViewLayout)_config.MultiviewerConfig.Layout);
            _BMDSwitcherMultiView.SetWindowInput(2, _config.MultiviewerConfig.Window2);
            _BMDSwitcherMultiView.SetWindowInput(3, _config.MultiviewerConfig.Window3);
            _BMDSwitcherMultiView.SetWindowInput(4, _config.MultiviewerConfig.Window4);
            _BMDSwitcherMultiView.SetWindowInput(5, _config.MultiviewerConfig.Window5);
            _BMDSwitcherMultiView.SetWindowInput(6, _config.MultiviewerConfig.Window6);
            _BMDSwitcherMultiView.SetWindowInput(7, _config.MultiviewerConfig.Window7);
            _BMDSwitcherMultiView.SetWindowInput(8, _config.MultiviewerConfig.Window8);
            _BMDSwitcherMultiView.SetWindowInput(9, _config.MultiviewerConfig.Window9);

            // disable all vu meters
            for (int i = 0; i < 10; i++)
            {
                _BMDSwitcherMultiView.SetVuMeterEnabled((uint)i, _config.MultiviewerConfig.ShowVUMetersOnWindows.Contains(i) ? 1 : 0);
            }
        }

        private void ConfigureUpstreamKey()
        {
            if (_config.USKSettings.IsChroma == 1)
            {
                ConfigureUSKforChroma();
                ConfigureDVEPIPSettings();
            }
            else if (_config.USKSettings.IsDVE == 1)
            {
                ConfigureUSKforPictureInPicture();
                ConfigureUSK1ChromaSettings();
            }
        }

        private void ConfigureUSKforChroma()
        {
            _BMDSwitcherUpstreamKey1.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeChroma);

            ConfigureUSK1ChromaSettings();

            ForceStateUpdate();
        }

        private void ConfigureUSK1ChromaSettings()
        {
            // set initial fill to slide
            _BMDSwitcherUpstreamKey1.SetInputFill(_config.USKSettings.ChromaSettings.FillSource);

            IBMDSwitcherKeyChromaParameters chromaParameters = (IBMDSwitcherKeyChromaParameters)_BMDSwitcherUpstreamKey1;

            chromaParameters.SetHue(_config.USKSettings.ChromaSettings.Hue);
            chromaParameters.SetGain(_config.USKSettings.ChromaSettings.Gain);
            chromaParameters.SetYSuppress(_config.USKSettings.ChromaSettings.YSuppress);
            chromaParameters.SetLift(_config.USKSettings.ChromaSettings.Lift);
            chromaParameters.SetNarrow(_config.USKSettings.ChromaSettings.Narrow);

        }

        private void ForceStateUpdate_ChromaSettings()
        {
            IBMDSwitcherKeyChromaParameters chromaParameters = (IBMDSwitcherKeyChromaParameters)_BMDSwitcherUpstreamKey1;
            double hue, gain, lift, ysuppress;
            int narrow;
            chromaParameters.GetHue(out hue);
            chromaParameters.GetGain(out gain);
            chromaParameters.GetLift(out lift);
            chromaParameters.GetYSuppress(out ysuppress);
            chromaParameters.GetNarrow(out narrow);
            _state.ChromaSettings = new BMDUSKChromaSettings()
            {
                FillSource = (int)_state.USK1FillSource,
                Hue = hue,
                Gain = gain,
                Lift = lift,
                YSuppress = ysuppress,
                Narrow = narrow
            };
        }

        private void ConfigureUSKforPictureInPicture()
        {
            _BMDSwitcherUpstreamKey1.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeDVE);

            ConfigureDVEPIPSettings();

            ForceStateUpdate();
        }

        private void ConfigureDVEPIPSettings()
        {
            // set initial fill source to center
            _BMDSwitcherUpstreamKey1.SetInputFill(_config.USKSettings.PIPSettings.DefaultFillSource);

            IBMDSwitcherKeyDVEParameters dveparams = (IBMDSwitcherKeyDVEParameters)_BMDSwitcherUpstreamKey1;

            // set border with dveparams
            dveparams.SetMasked(_config.USKSettings.PIPSettings.IsMasked);
            dveparams.SetMaskTop(_config.USKSettings.PIPSettings.MaskTop);
            dveparams.SetMaskBottom(_config.USKSettings.PIPSettings.MaskBottom);
            dveparams.SetMaskLeft(_config.USKSettings.PIPSettings.MaskLeft);
            dveparams.SetMaskRight(_config.USKSettings.PIPSettings.MaskRight);


            dveparams.SetBorderEnabled(_config.USKSettings.PIPSettings.IsBordered);


            // config size & base position
            _BMDSwitcherFlyKeyParamters.SetPositionX(_config.USKSettings.PIPSettings.Current.PositionX);
            _BMDSwitcherFlyKeyParamters.SetPositionY(_config.USKSettings.PIPSettings.Current.PositionY);
            _BMDSwitcherFlyKeyParamters.SetSizeX(_config.USKSettings.PIPSettings.Current.SizeX);
            _BMDSwitcherFlyKeyParamters.SetSizeY(_config.USKSettings.PIPSettings.Current.SizeY);

            // setup keyframes
            IBMDSwitcherKeyFlyKeyFrameParameters keyframeparams;
            _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out keyframeparams);

            keyframeparams.SetPositionX(_config.USKSettings.PIPSettings.KeyFrameA.PositionX);
            keyframeparams.SetPositionY(_config.USKSettings.PIPSettings.KeyFrameA.PositionY);
            keyframeparams.SetSizeX(_config.USKSettings.PIPSettings.KeyFrameA.SizeX);
            keyframeparams.SetSizeY(_config.USKSettings.PIPSettings.KeyFrameA.SizeY);

            _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out keyframeparams);

            keyframeparams.SetPositionX(_config.USKSettings.PIPSettings.KeyFrameB.PositionX);
            keyframeparams.SetPositionY(_config.USKSettings.PIPSettings.KeyFrameB.PositionY);
            keyframeparams.SetSizeX(_config.USKSettings.PIPSettings.KeyFrameB.SizeX);
            keyframeparams.SetSizeY(_config.USKSettings.PIPSettings.KeyFrameB.SizeY);
        }

        private void ForceStateUpdate_PIPSettings()
        {
            _parent.Dispatcher.Invoke(() =>
            {
                IBMDSwitcherKeyDVEParameters dveparams = (IBMDSwitcherKeyDVEParameters)_BMDSwitcherUpstreamKey1;

                int isbordered;
                int ismasked;
                double masktop, maskbot, maskleft, maskright;
                dveparams.GetMasked(out ismasked);
                dveparams.GetMaskBottom(out maskbot);
                dveparams.GetMaskTop(out masktop);
                dveparams.GetMaskLeft(out maskleft);
                dveparams.GetMaskRight(out maskright);
                dveparams.GetBorderEnabled(out isbordered);

                double cposx, cposy, csizex, csizey;
                _BMDSwitcherFlyKeyParamters.GetPositionX(out cposx);
                _BMDSwitcherFlyKeyParamters.GetPositionY(out cposy);
                _BMDSwitcherFlyKeyParamters.GetSizeX(out csizex);
                _BMDSwitcherFlyKeyParamters.GetSizeY(out csizey);

                double aposx, aposy, asizex, asizey;
                IBMDSwitcherKeyFlyKeyFrameParameters keyframeparamsa;
                _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out keyframeparamsa);
                keyframeparamsa.GetPositionX(out aposx);
                keyframeparamsa.GetPositionY(out aposy);
                keyframeparamsa.GetSizeX(out asizex);
                keyframeparamsa.GetSizeY(out asizey);

                double bposx, bposy, bsizex, bsizey;
                IBMDSwitcherKeyFlyKeyFrameParameters keyframeparamsb;
                _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out keyframeparamsb);
                keyframeparamsb.GetPositionX(out bposx);
                keyframeparamsb.GetPositionY(out bposy);
                keyframeparamsb.GetSizeX(out bsizex);
                keyframeparamsb.GetSizeY(out bsizey);


                _state.DVESettings = new BMDUSKDVESettings()
                {
                    Current = new KeyFrameSettings()
                    {
                        PositionX = cposx,
                        PositionY = cposy,
                        SizeX = csizex,
                        SizeY = csizey
                    },
                    DefaultFillSource = _config?.USKSettings.PIPSettings.DefaultFillSource ?? 0,
                    IsBordered = isbordered,
                    IsMasked = ismasked,
                    MaskBottom = (float)maskbot,
                    MaskTop = (float)masktop,
                    MaskLeft = (float)maskleft,
                    MaskRight = (float)maskright,
                    KeyFrameA = new KeyFrameSettings()
                    {
                        PositionX = aposx,
                        PositionY = aposy,
                        SizeX = asizex,
                        SizeY = asizey
                    },
                    KeyFrameB = new KeyFrameSettings()
                    {
                        PositionX = bposx,
                        PositionY = bposy,
                        SizeX = bsizex,
                        SizeY = bsizey
                    }
                };
            });
        }

        private void ConfigureAudioLevels()
        {
            IBMDSwitcherAudioMixer mixer = (IBMDSwitcherAudioMixer)_BMDSwitcher;

            mixer.SetProgramOutGain(_config.AudioSettings.ProgramOutGain);

            IBMDSwitcherAudioInputIterator audioIterator = null;
            IntPtr audioPtr;
            Guid audioInputIID = typeof(IBMDSwitcherAudioInputIterator).GUID;
            mixer.CreateIterator(ref audioInputIID, out audioPtr);
            if (audioPtr != null)
            {
                audioIterator = (IBMDSwitcherAudioInputIterator)Marshal.GetObjectForIUnknown(audioPtr);

                IBMDSwitcherAudioInput xlr;
                audioIterator.GetById((long)BMDSwitcherAudioSources.XLR, out xlr);
                xlr.SetGain(_config.AudioSettings.XLRInputGain);

                // for now disable all other inputs
                IBMDSwitcherAudioInput audioinput;
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_1, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_2, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_3, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_4, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_5, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_6, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_7, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);
                audioIterator.GetById((long)BMDSwitcherAudioSources.Input_8, out audioinput);
                audioinput.SetMixOption(_BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff);

            }

        }


        public void PerformPresetSelect(int sourceID)
        {
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.SetPreviewInput((long)sourceID);
            }
        }

        public void PerformProgramSelect(int sourceID)
        {
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.SetProgramInput((long)sourceID);
            }
        }

        public void PerformCutTransition()
        {
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.PerformCut();
            }
        }

        public void PerformAutoTransition()
        {
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.PerformAutoTransition();
            }
        }

        public void PerformToggleUSK1()
        {
            _BMDSwitcherUpstreamKey1.SetOnAir(_state.USK1OnAir ? 0 : 1);
        }

        public void PerformTieUSK1()
        {
            //_BMDSwitcherUpstreamKey1.set
        }

        public void PerformToggleDSK1()
        {
            _BMDSwitcherDownstreamKey1.SetOnAir(_state.DSK1OnAir ? 0 : 1);
        }
        public void PerformTieDSK1()
        {
            _BMDSwitcherDownstreamKey1.SetTie(_state.DSK1Tie ? 0 : 1);
        }
        public void PerformTakeAutoDSK1()
        {
            _BMDSwitcherDownstreamKey1.PerformAutoTransition();
        }
        public void PerformAutoOnAirDSK1()
        {
            _BMDSwitcherDownstreamKey1.PerformAutoTransitionInDirection(1);
        }
        public void PerformAutoOffAirDSK1()
        {
            _BMDSwitcherDownstreamKey1.PerformAutoTransitionInDirection(0);
        }
        public void PerformToggleDSK2()
        {
            _BMDSwitcherDownstreamKey2.SetOnAir(_state.DSK2OnAir ? 0 : 1);
        }
        public void PerformTieDSK2()
        {
            _BMDSwitcherDownstreamKey2.SetTie(_state.DSK2Tie ? 0 : 1);
        }
        public void PerformTakeAutoDSK2()
        {
            _BMDSwitcherDownstreamKey2.PerformAutoTransition();
        }
        public void PerformAutoOnAirDSK2()
        {
            _BMDSwitcherDownstreamKey2.PerformAutoTransitionInDirection(1);
        }
        public void PerformAutoOffAirDSK2()
        {
            _BMDSwitcherDownstreamKey2.PerformAutoTransitionInDirection(0);
        }

        public void PerformToggleFTB()
        {
            _BMDSwitcherMixEffectBlock1.PerformFadeToBlack();
        }

        public void Close()
        {
            SwitcherDisconnected();
        }

        public void PerformUSK1RunToKeyFrameA()
        {
            _BMDSwitcherFlyKeyParamters.RunToKeyFrame(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA);
        }

        public void PerformUSK1RunToKeyFrameB()
        {
            _BMDSwitcherFlyKeyParamters.RunToKeyFrame(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB);
        }

        public void PerformUSK1RunToKeyFrameFull()
        {
            _BMDSwitcherFlyKeyParamters.RunToKeyFrame(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameFull);
        }

        public void PerformUSK1FillSourceSelect(int sourceID)
        {
            _BMDSwitcherUpstreamKey1.SetInputFill(sourceID);
        }

        void IBMDSwitcherManager.PerformToggleBackgroundForNextTrans()
        {
            // only allow deselection if at least one layer is selected
            if (_state.TransNextBackground)
            {
                // to disable background key1 needs to be selected
                if (!_state.TransNextKey1)
                {
                    // dont' do anything
                    return;
                }
            }


            int val = 0;
            if (!_state.TransNextBackground)
            {
                val |= (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground;
            }
            if (_state.TransNextKey1)
            {
                val |= (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1;
            }
            _BMDSwitcherTransitionParameters.SetNextTransitionSelection((_BMDSwitcherTransitionSelection)val);
        }

        void IBMDSwitcherManager.PerformToggleKey1ForNextTrans()
        {


            // only allow deselection if at least one layer is selected
            if (_state.TransNextKey1)
            {
                // to disable key1 background needs to be selected
                if (!_state.TransNextBackground)
                {
                    // don't do anything
                    return;
                }
            }


            int val = 0;
            if (_state.TransNextBackground)
            {
                val |= (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground;
            }
            if (!_state.TransNextKey1)
            {
                val |= (int)_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1;
            }
            _BMDSwitcherTransitionParameters.SetNextTransitionSelection((_BMDSwitcherTransitionSelection)val);
        }

        public void SetPIPPosition(BMDUSKDVESettings settings)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                IBMDSwitcherKeyDVEParameters dveparams = (IBMDSwitcherKeyDVEParameters)_BMDSwitcherUpstreamKey1;

                // set border with dveparams
                dveparams.SetMasked(settings.IsMasked);
                dveparams.SetMaskTop(settings.MaskTop);
                dveparams.SetMaskBottom(settings.MaskBottom);
                dveparams.SetMaskLeft(settings.MaskLeft);
                dveparams.SetMaskRight(settings.MaskRight);


                dveparams.SetBorderEnabled(settings.IsBordered);


                // config size & base position
                _BMDSwitcherFlyKeyParamters.SetPositionX(settings.Current.PositionX);
                _BMDSwitcherFlyKeyParamters.SetPositionY(settings.Current.PositionY);
                _BMDSwitcherFlyKeyParamters.SetSizeX(settings.Current.SizeX);
                _BMDSwitcherFlyKeyParamters.SetSizeY(settings.Current.SizeY);

                ForceStateUpdate();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        public void SetPIPKeyFrameA(BMDUSKDVESettings settings)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                {
                    IBMDSwitcherKeyDVEParameters dveparams = (IBMDSwitcherKeyDVEParameters)_BMDSwitcherUpstreamKey1;

                    // set border with dveparams
                    dveparams.SetMasked(settings.IsMasked);
                    dveparams.SetMaskTop(settings.MaskTop);
                    dveparams.SetMaskBottom(settings.MaskBottom);
                    dveparams.SetMaskLeft(settings.MaskLeft);
                    dveparams.SetMaskRight(settings.MaskRight);


                    dveparams.SetBorderEnabled(settings.IsBordered);

                    // setup keyframes
                    IBMDSwitcherKeyFlyKeyFrameParameters keyframeparams;
                    _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out keyframeparams);

                    keyframeparams.SetPositionX(settings.KeyFrameA.PositionX);
                    keyframeparams.SetPositionY(settings.KeyFrameA.PositionY);
                    keyframeparams.SetSizeX(settings.KeyFrameA.SizeX);
                    keyframeparams.SetSizeY(settings.KeyFrameA.SizeY);
                }


                ForceStateUpdate();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        public void SetPIPKeyFrameB(BMDUSKDVESettings settings)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                {
                    IBMDSwitcherKeyDVEParameters dveparams = (IBMDSwitcherKeyDVEParameters)_BMDSwitcherUpstreamKey1;

                    // set border with dveparams
                    dveparams.SetMasked(settings.IsMasked);
                    dveparams.SetMaskTop(settings.MaskTop);
                    dveparams.SetMaskBottom(settings.MaskBottom);
                    dveparams.SetMaskLeft(settings.MaskLeft);
                    dveparams.SetMaskRight(settings.MaskRight);


                    dveparams.SetBorderEnabled(settings.IsBordered);

                    // setup keyframes
                    IBMDSwitcherKeyFlyKeyFrameParameters keyframeparams;
                    _BMDSwitcherFlyKeyParamters.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out keyframeparams);

                    keyframeparams.SetPositionX(settings.KeyFrameB.PositionX);
                    keyframeparams.SetPositionY(settings.KeyFrameB.PositionY);
                    keyframeparams.SetSizeX(settings.KeyFrameB.SizeX);
                    keyframeparams.SetSizeY(settings.KeyFrameB.SizeY);
                }


                ForceStateUpdate();
                SwitcherStateChanged?.Invoke(_state);
            });
        }


        public void ConfigureUSK1PIP(BMDUSKDVESettings settings)
        {
            _config.USKSettings.IsDVE = 1;
            _config.USKSettings.IsChroma = 0;
            _config.USKSettings.PIPSettings = settings;
            ConfigureUSKforPictureInPicture();

            ForceStateUpdate();
        }

        public void ConfigureUSK1Chroma(BMDUSKChromaSettings settings)
        {
            _config.USKSettings.IsDVE = 0;
            _config.USKSettings.IsChroma = 1;
            _config.USKSettings.ChromaSettings = settings;
            ConfigureUSKforChroma();


            ForceStateUpdate();
        }

        public void PerformOnAirUSK1()
        {
            _parent.Dispatcher.Invoke(() =>
            {
                _BMDSwitcherUpstreamKey1.SetOnAir(1);
            });
        }

        public void PerformOffAirUSK1()
        {
            _parent.Dispatcher.Invoke(() =>
            {
                _BMDSwitcherUpstreamKey1.SetOnAir(0);
            });
        }

        public void SetUSK1TypeDVE()
        {
            _parent.Dispatcher.Invoke(() =>
            {
                _BMDSwitcherUpstreamKey1.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeDVE);
                ForceStateUpdate();
            });
        }

        public void SetUSK1TypeChroma()
        {
            _parent.Dispatcher.Invoke(() =>
            {
                _BMDSwitcherUpstreamKey1.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeChroma);
                ForceStateUpdate();
            });
        }

        public void PerformSetKey1OnForNextTrans()
        {

        }

        public void PerformSetKey1OffForNextTrans()
        {

        }

        public void PerformAuxSelect(int sourceID)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                _BMDSwitcherAuxInput?.SetInputSource(sourceID);
                ForceStateUpdate();
            });
        }
    }
}
