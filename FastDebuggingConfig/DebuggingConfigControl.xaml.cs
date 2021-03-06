﻿//------------------------------------------------------------------------------
// <copyright file="DebuggingConfigControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace FastDebuggingConfig
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.VCProjectEngine;
    using System;
    using System.Timers;
    using System.Windows.Controls;
    public partial class DebuggingConfigControl : UserControl
    {
        private System.Timers.Timer wdTimer = new System.Timers.Timer();
        private System.Timers.Timer caTimer = new System.Timers.Timer();

        private string currentWorkingDirectory = "";
        private string currentCommandArguments = "";


        private IVsSolution _solution;
        private SolutionEvents solutionEvents;

        public DebuggingConfigControl()
        {
            this.InitializeComponent();

            solutionEvents = GetDTE2().Events.SolutionEvents;
            solutionEvents.Opened += new _dispSolutionEvents_OpenedEventHandler(SolutionEvents_Opened);

            wdTimer.Elapsed += new ElapsedEventHandler(OnTimedEventWD);
            wdTimer.Enabled = false;

            caTimer.Elapsed += new ElapsedEventHandler(OnTimedEventCA);
            caTimer.Enabled = false;
        }

        private static DTE2 GetDTE2()
        {
            return Package.GetGlobalService(typeof(DTE)) as DTE2;
        }

        private void SolutionEvents_Opened()
        {
            UpdateTextBoxes();
        }

        private void UpdateTextBoxes()
        {
            Project startupProj = GetStartUpProject();
            if (startupProj != null)
            {
                VCProject vcProj = startupProj.Object as VCProject;
                if (vcProj != null)
                {
                    foreach (VCConfiguration config in vcProj.Configurations)
                    {
                        VCDebugSettings debugSetting = config.DebugSettings;
                        textboxwd.Text = debugSetting.WorkingDirectory;
                        textboxcla.Text = debugSetting.CommandArguments;
                    }
                }
                else
                {
                    Configuration configuration = startupProj.ConfigurationManager.ActiveConfiguration;
                    textboxwd.Text = configuration.Properties.Item("StartWorkingDirectory").Value.ToString();
                    textboxcla.Text = configuration.Properties.Item("StartArguments").Value.ToString();
                }
            }
        }

        private void OnTimedEventWD(object source, ElapsedEventArgs e)
        {
            SetWorkingDirectory(currentWorkingDirectory);
            wdTimer.Stop();
        }

        private void OnTimedEventCA(object source, ElapsedEventArgs e)
        {
            SetCommandArguments(currentCommandArguments);
            caTimer.Stop();
        }

        private void TextBox_TextChanged_WD(object sender, TextChangedEventArgs e)
        {
            currentWorkingDirectory = textboxwd.Text;
            wdTimer.Interval = 1000;
            wdTimer.Start();
        }

        private void TextBox_TextChanged_CLA(object sender, TextChangedEventArgs e)
        {
            currentCommandArguments = textboxcla.Text;
            caTimer.Interval = 1000;
            caTimer.Start();
        }

        private Project GetStartUpProject()
        {
            DTE2 dte2 = GetDTE2();
            SolutionBuild2 sb = (SolutionBuild2)dte2.Solution.SolutionBuild;
            
            if(sb.StartupProjects != null)
            {
                string name = "";
                foreach (String s in (Array)sb.StartupProjects)
                {
                    name += s;
                }

                if(string.IsNullOrEmpty(name))
                {
                    return null;
                }

                return dte2.Solution.Item(name);
            }

            return null;
        }

       
        private void SetCommandArguments(string value)
        {
            Project startupProj = GetStartUpProject();
            if (startupProj != null)
            {
                VCProject vcProj = startupProj.Object as VCProject;
                if (vcProj != null)
                {
                    foreach (VCConfiguration config in vcProj.Configurations)
                    {
                        VCDebugSettings debugSetting = config.DebugSettings;
                        debugSetting.CommandArguments = value;
                    }
                }
                else
                {
                    Configuration configuration = startupProj.ConfigurationManager.ActiveConfiguration;
                    configuration.Properties.Item("StartArguments").Value = value;
                }
            }
        }

        private void SetWorkingDirectory(string value)
        {
            Project startupProj = GetStartUpProject();
            if (startupProj != null)
            {
                VCProject vcProj = startupProj.Object as VCProject;
                if (vcProj != null)
                {
                    foreach (VCConfiguration config in vcProj.Configurations)
                    {
                        VCDebugSettings debugSetting = config.DebugSettings;
                        debugSetting.WorkingDirectory = value;
                    }
                }
                else
                {
                    Configuration configuration = startupProj.ConfigurationManager.ActiveConfiguration;
                    configuration.Properties.Item("StartWorkingDirectory").Value = value;
                }
            }
        }

        private void Grid_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateTextBoxes();
        }
    }
}