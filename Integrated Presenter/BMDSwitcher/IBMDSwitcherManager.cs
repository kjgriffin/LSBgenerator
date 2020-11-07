﻿using Integrated_Presenter.BMDSwitcher;
using Integrated_Presenter.BMDSwitcher.Config;

namespace Integrated_Presenter
{
    public interface IBMDSwitcherManager
    {
        bool GoodConnection { get; set; }

        event SwitcherStateChange SwitcherStateChanged;

        BMDSwitcherState ForceStateUpdate();
        BMDSwitcherState GetCurrentState();
        void PerformAutoOffAirDSK2();
        void PerformAutoOnAirDSK2();
        void PerformAutoOffAirDSK1();
        void PerformAutoOnAirDSK1();
        void PerformAutoTransition();
        void PerformCutTransition();
        void PerformPresetSelect(int sourceID);
        void PerformProgramSelect(int sourceID);
        void PerformTakeAutoDSK1();
        void PerformTakeAutoDSK2();
        void PerformTieDSK1();
        void PerformTieDSK2();
        void PerformToggleDSK1();
        void PerformToggleDSK2();
        void PerformToggleFTB();
        bool TryConnect(string address);
        void PerformToggleUSK1();
        void PerformUSK1RunToKeyFrameA();
        void PerformUSK1RunToKeyFrameB();
        void PerformUSK1RunToKeyFrameFull();
        void PerformUSK1FillSourceSelect(int sourceID);
        void PerformToggleBackgroundForNextTrans();
        void PerformToggleKey1ForNextTrans();

        void SetPIPPosition(BMDUSKSettings settings);
        void SetPIPKeyFrameA(BMDUSKSettings settings);

        void Close();
        void ConfigureSwitcher(BMDSwitcherConfigSettings config);
    }
}