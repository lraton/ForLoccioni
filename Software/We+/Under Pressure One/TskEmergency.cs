﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace InterfacciaUtente
{
    public class TskEmergency
    {
        public event EventHandler<string> EmergencyUpdatedEvent;
        public event EventHandler CloseFormEmergency;

        TskHwComunication tskHwComunication;
        TskReset tskReset;
        GetStatus getStatus;

        Timer tmr;

        string phaseEmergency = "";
        public bool Started;
        public string msgUser = "";
        
        public TskEmergency()
        {
            tmr = new Timer();
            tmr.Interval = 200;
            tmr.Enabled = false;
            tmr.Elapsed += Tmr_Elapsed;
            Started = false;
        }


        private void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            tmr.Enabled = false;
            if (getStatus.led)
            {
                switch (phaseEmergency)
                {

                    case "eBegin":

                        if (getStatus.StateAir)
                        {
                            tskHwComunication.CloseAir();
                            phaseEmergency = "wfCloseAir";
                        }
                        else
                        {
                            phaseEmergency = "CloseVoltage";
                        }
                        break;


                    case "wfCloseAir":
                        if (!getStatus.StateAir)
                        {
                            phaseEmergency = "CloseVoltage";
                        }
                        break;


                    case "CloseVoltage":
                        if (getStatus.Volt >= 1)
                        {
                            tskHwComunication.SetVoltage(0);
                            phaseEmergency = "wfCloseVoltage";
                        }
                        else
                        {
                            phaseEmergency = "userMessage";
                        }
                        break;



                    case "wfCloseVoltage":
                        if (getStatus.Volt <= 1)
                        {
                            phaseEmergency = "userMessage";
                        }
                        break;


                    case "userMessage":
                        
                            if (getStatus.StateEmergency)
                            {
                                msgUser = "Unlock Button Emergency";
                            }
                            else
                            {
                                msgUser = "Press Reset";
                                phaseEmergency = "wfPressReset";
                            }
                            EmergencyUpdatedEvent.Invoke(this, msgUser);
                        
                        break;



                    case "wfPressReset":
                        if (getStatus.StateReset)
                        {
                            SwitchOff();
                            CloseFormEmergency.Invoke(this, e);
                            tskReset.SwitchOn();
                            phaseEmergency = "doNothing";
                            return;
                        }
                        break;


                    case "doNothing":
                        break;

                }
            }
            tmr.Enabled = true;

        }

        public void InitData(ref GetStatus _getStatus, ref TskHwComunication _tskHw, ref TskReset _tskReset)
        {
            getStatus = _getStatus;
            tskHwComunication = _tskHw;
            tskReset = _tskReset;
        }

        public void SwitchOn()
        {
            tmr.Enabled = true;
            phaseEmergency = "eBegin";
            Started = true;
        }

        public void SwitchOff()
        {
            tmr.Enabled = false;
            phaseEmergency = "doNothing";
            Started = false;
        }
    }
}
