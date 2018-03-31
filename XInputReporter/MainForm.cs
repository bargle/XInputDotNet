﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using XInputDotNetPure;
using System.Drawing;
using System.Runtime.InteropServices;

namespace XInputReporter
{
    public partial class MainForm : Form
    {
        private ReporterState reporterState = new ReporterState();
        private Control[] controllerControls;
        private Control[] stickControls;
        private Point[] stickControlPositions;
		float m_x = 0.0f;
		float m_y = 0.0f;

		[DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public MainForm()
        {
            InitializeComponent();

            controllerControls = new Control[] {
                picController1,
                picController2,
                picController3,
                picController4
            };
            stickControls = new Control[] {
                picStickLeft,
                picStickRight
            };
            stickControlPositions = new Point[] {
                picStickLeft.Location,
                picStickRight.Location
            };
        }

        private void pollingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                if (reporterState.Poll())
                {
					try
					{ 
						Invoke(new Action(UpdateState));
					}
					catch(Exception) { }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            comboDeadZone.Items.AddRange(Enum.GetNames(typeof(GamePadDeadZone)));
            comboDeadZone.SelectedItem = reporterState.DeadZone.ToString();

            pollingWorker.RunWorkerAsync();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            pollingWorker.CancelAsync();
        }

        private static string FormatFloat(float v)
        {
            return string.Format("{0:F3}", v);
        }

        private static void PositionStickControl(Control control, Point location, GamePadThumbSticks.StickValue value)
        {
            var deltaX = value.X * control.Width * 0.5f;
            var deltaY = value.Y * control.Height * -0.5f;

            control.Location = new Point(location.X + (int)deltaX, location.Y + (int)deltaY);
            control.Refresh();
        }

        private void UpdateState()
        {
            checkA.Checked = reporterState.LastActiveState.Buttons.A == XInputDotNetPure.ButtonState.Pressed;
            checkB.Checked = reporterState.LastActiveState.Buttons.B == XInputDotNetPure.ButtonState.Pressed;
            checkX.Checked = reporterState.LastActiveState.Buttons.X == XInputDotNetPure.ButtonState.Pressed;
            checkY.Checked = reporterState.LastActiveState.Buttons.Y == XInputDotNetPure.ButtonState.Pressed;
            checkStart.Checked = reporterState.LastActiveState.Buttons.Start == XInputDotNetPure.ButtonState.Pressed;
            checkBack.Checked = reporterState.LastActiveState.Buttons.Back == XInputDotNetPure.ButtonState.Pressed;
            checkGuide.Checked = reporterState.LastActiveState.Buttons.Guide == XInputDotNetPure.ButtonState.Pressed;
            checkStickLeft.Checked = reporterState.LastActiveState.Buttons.LeftStick == XInputDotNetPure.ButtonState.Pressed;
            checkStickRight.Checked = reporterState.LastActiveState.Buttons.RightStick == XInputDotNetPure.ButtonState.Pressed;
            checkShoulderLeft.Checked = reporterState.LastActiveState.Buttons.LeftShoulder == XInputDotNetPure.ButtonState.Pressed;
            checkShoulderRight.Checked = reporterState.LastActiveState.Buttons.RightShoulder == XInputDotNetPure.ButtonState.Pressed;

            checkDPadUp.Checked = reporterState.LastActiveState.DPad.Up == XInputDotNetPure.ButtonState.Pressed;
            checkDPadRight.Checked = reporterState.LastActiveState.DPad.Right == XInputDotNetPure.ButtonState.Pressed;
            checkDPadDown.Checked = reporterState.LastActiveState.DPad.Down == XInputDotNetPure.ButtonState.Pressed;
            checkDPadLeft.Checked = reporterState.LastActiveState.DPad.Left == XInputDotNetPure.ButtonState.Pressed;

            labelTriggerLeft.Text = FormatFloat(reporterState.LastActiveState.Triggers.Left);
            labelTriggerRight.Text = FormatFloat(reporterState.LastActiveState.Triggers.Right);

            labelStickLeftX.Text = FormatFloat(reporterState.LastActiveState.ThumbSticks.Left.X);
            labelStickLeftY.Text = FormatFloat(reporterState.LastActiveState.ThumbSticks.Left.Y);
            labelStickRightX.Text = FormatFloat(reporterState.LastActiveState.ThumbSticks.Right.X);
            labelStickRightY.Text = FormatFloat(reporterState.LastActiveState.ThumbSticks.Right.Y);

            if (reporterState.LastActiveState.Buttons.Start == XInputDotNetPure.ButtonState.Pressed)
            {
                timerStart.Start();
            }
            else
            {
                timerStart.Stop();
            }
            if (reporterState.LastActiveState.Buttons.Back == XInputDotNetPure.ButtonState.Pressed)
            {
                timerBack.Start();
            }
            else
            {
                timerBack.Stop();
            }

            for (int i = 0; i < 4; i++)
            {
                controllerControls[i].Visible = i == reporterState.LastActiveIndex && reporterState.LastActiveState.IsConnected;
            }

            PositionStickControl(stickControls[0], stickControlPositions[0], reporterState.LastActiveState.ThumbSticks.Left);
            PositionStickControl(stickControls[1], stickControlPositions[1], reporterState.LastActiveState.ThumbSticks.Right);

			if ( m_x != reporterState.LastActiveState.ThumbSticks.Left.X )
			{
				// A
				if ( m_x < -0.24f ) //was left KB_DN
				{
					if ( reporterState.LastActiveState.ThumbSticks.Left.X > -0.24f ) // is now KB_UP
					{
						keybd_event( 0x41, 0, 0x0002, 0);
					}
				}

				if ( m_x > -0.24f ) //was left KB_UP
				{
					if ( reporterState.LastActiveState.ThumbSticks.Left.X < -0.24f ) // is now KB_DN
					{
						keybd_event( 0x41, 0, 0, 0);
					}
				}

				// D
				if ( m_x > 0.24f ) //was left KB_DN
				{
					if ( reporterState.LastActiveState.ThumbSticks.Left.X < 0.24f ) // is now KB_UP
					{
						keybd_event( 0x44, 0, 0x0002, 0);
					}
				}

				if ( m_x < 0.24f ) //was left KB_UP
				{
					if ( reporterState.LastActiveState.ThumbSticks.Left.X > 0.24f ) // is now KB_DN
					{
						keybd_event( 0x44, 0, 0, 0);
					}
				}

			}

			if ( m_y != reporterState.LastActiveState.ThumbSticks.Left.Y )
			{ 

				// S
				if ( m_y < -0.24f ) //was down KB_DN
				{
					if ( reporterState.LastActiveState.ThumbSticks.Left.Y > -0.24f ) // is now KB_UP
					{
						keybd_event( 0x53, 0, 0x0002, 0);
					}
				}

				if ( m_y > -0.24f ) //was down KB_UP
				{
					if ( reporterState.LastActiveState.ThumbSticks.Left.Y < -0.24f ) // is now KB_DN
					{
						keybd_event( 0x53, 0, 0, 0);
					}
				}

				// W
				if ( m_y > 0.24f ) //was left KB_DN
				{
					if ( reporterState.LastActiveState.ThumbSticks.Left.Y < 0.24f ) // is now KB_UP
					{
						keybd_event( 0x57, 0, 0x0002, 0);
					}
				}

				if ( m_y < 0.24f ) //was left KB_UP
				{
					if ( reporterState.LastActiveState.ThumbSticks.Left.Y > 0.24f ) // is now KB_DN
					{
						keybd_event( 0x57, 0, 0, 0);
					}
				}

			}

			m_x = reporterState.LastActiveState.ThumbSticks.Left.X;
			m_y = reporterState.LastActiveState.ThumbSticks.Left.Y;

        }

        private void checkLink_CheckedChanged(object sender, EventArgs e)
        {
            reporterState.LinkTriggersToVibration = checkLink.Checked;
        }

        private void comboDeadZone_SelectedValueChanged(object sender, EventArgs e)
        {
            reporterState.DeadZone = (GamePadDeadZone)Enum.Parse(typeof(GamePadDeadZone), comboDeadZone.SelectedItem as string);
        }

        private void timerStart_Tick(object sender, EventArgs e)
        {
            var deadZoneInt = (int)reporterState.DeadZone;
            deadZoneInt = (deadZoneInt + 1) % Enum.GetValues(typeof(GamePadDeadZone)).Length;
            comboDeadZone.SelectedItem = ((GamePadDeadZone)deadZoneInt).ToString();
            timerStart.Stop();
        }

        private void timerBack_Tick(object sender, EventArgs e)
        {
            checkLink.Checked = !checkLink.Checked;
            timerBack.Stop();
        }

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
		}
	}
}
