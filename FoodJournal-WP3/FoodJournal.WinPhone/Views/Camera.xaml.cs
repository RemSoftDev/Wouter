using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using Microsoft.Devices;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using System.Windows.Media;
using FoodJournal.Logging;
using System.IO;
using FoodJournal.AppModel.UI;
using FoodJournal.Runtime;

namespace FoodJournal.Views
{
    public partial class Camera : Screen
    {

        PhotoCamera cam;

        public Camera()
        {
            InitializeComponent();

            viewfinderBrush.RelativeTransform =
                new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 90 };

        }

        //Code for initialization, capture completed, image availability events; also setting the source for the viewfinder.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            try
            {
                if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
                {
                    cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);

                    cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);
                    //cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_CaptureCompleted);
                    cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);
                    cam.AutoFocusCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_AutoFocusCompleted);

                    viewfinderCanvas.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(focus_Tapped);

                    CameraButtons.ShutterKeyHalfPressed += OnButtonHalfPress;
                    CameraButtons.ShutterKeyPressed += OnButtonFullPress;
                    CameraButtons.ShutterKeyReleased += OnButtonRelease;

                    viewfinderBrush.SetSource(cam);
                }

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            try
            {
                if (cam != null)
                {
                    // Dispose camera to minimize power consumption and to expedite shutdown.
                    cam.Dispose();

                    // Release memory, ensure garbage collection.
                    cam.Initialized -= cam_Initialized;
                    cam.CaptureImageAvailable -= cam_CaptureImageAvailable;
                    //cam.CaptureThumbnailAvailable -= cam_CaptureThumbnailAvailable;
                    cam.AutoFocusCompleted -= cam_AutoFocusCompleted;
                    CameraButtons.ShutterKeyHalfPressed -= OnButtonHalfPress;
                    CameraButtons.ShutterKeyPressed -= OnButtonFullPress;
                    CameraButtons.ShutterKeyReleased -= OnButtonRelease;
                }
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        // Update the UI if initialization succeeds.
        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            try
            {
                if (e.Succeeded)
                {
                    cam.FlashMode = FlashMode.Off;
                    cam.Resolution = cam.AvailableResolutions.First();
                }
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            if (cam != null)
            {
                try
                {
                    // Start image capture.
                    cam.CaptureImage();
                }
                catch (Exception ex) { LittleWatson.ReportException(ex); }
            }
        }

        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {

            try
            {

                e.ImageStream.Seek(0, SeekOrigin.Begin);
                MemoryStream ms1 = new MemoryStream();
                e.ImageStream.CopyTo(ms1);

                // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {

                    try
                    {
                        bool isPortait = (this.Orientation == PageOrientation.Portrait || this.Orientation == PageOrientation.PortraitDown || this.Orientation == PageOrientation.PortraitUp);
                        PictureCache.Current.Save(ms1, Navigate.PictureFilename, Navigate.PictureFoodId, isPortait);
                        Navigate.BackFromOther();
                    }
                    catch (Exception ex) { LittleWatson.ReportException(ex); }
                    finally
                    {
                        ms1.Dispose();
                    }

                });

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }

        }

        //// Activate a flash mode.
        //// Cycle through flash mode options when the flash button is pressed.
        //private void changeFlash_Clicked(object sender, RoutedEventArgs e)
        //{

        //    switch (cam.FlashMode)
        //    {
        //        case FlashMode.Off:
        //            if (cam.IsFlashModeSupported(FlashMode.On))
        //            {
        //                // Specify that flash should be used.
        //                cam.FlashMode = FlashMode.On;
        //                FlashButton.Content = "Fl:On";
        //                currentFlashMode = "Flash mode: On";
        //            }
        //            break;
        //        case FlashMode.On:
        //            if (cam.IsFlashModeSupported(FlashMode.RedEyeReduction))
        //            {
        //                // Specify that the red-eye reduction flash should be used.
        //                cam.FlashMode = FlashMode.RedEyeReduction;
        //                FlashButton.Content = "Fl:RER";
        //                currentFlashMode = "Flash mode: RedEyeReduction";
        //            }
        //            else if (cam.IsFlashModeSupported(FlashMode.Auto))
        //            {
        //                // If red-eye reduction is not supported, specify automatic mode.
        //                cam.FlashMode = FlashMode.Auto;
        //                FlashButton.Content = "Fl:Auto";
        //                currentFlashMode = "Flash mode: Auto";
        //            }
        //            else
        //            {
        //                // If automatic is not supported, specify that no flash should be used.
        //                cam.FlashMode = FlashMode.Off;
        //                FlashButton.Content = "Fl:Off";
        //                currentFlashMode = "Flash mode: Off";
        //            }
        //            break;
        //        case FlashMode.RedEyeReduction:
        //            if (cam.IsFlashModeSupported(FlashMode.Auto))
        //            {
        //                // Specify that the flash should be used in the automatic mode.
        //                cam.FlashMode = FlashMode.Auto;
        //                FlashButton.Content = "Fl:Auto";
        //                currentFlashMode = "Flash mode: Auto";
        //            }
        //            else
        //            {
        //                // If automatic is not supported, specify that no flash should be used.
        //                cam.FlashMode = FlashMode.Off;
        //                FlashButton.Content = "Fl:Off";
        //                currentFlashMode = "Flash mode: Off";
        //            }
        //            break;
        //        case FlashMode.Auto:
        //            if (cam.IsFlashModeSupported(FlashMode.Off))
        //            {
        //                // Specify that no flash should be used.
        //                cam.FlashMode = FlashMode.Off;
        //                FlashButton.Content = "Fl:Off";
        //                currentFlashMode = "Flash mode: Off";
        //            }
        //            break;
        //    }

        //    // Display current flash mode.
        //    this.Dispatcher.BeginInvoke(delegate()
        //    {
        //        txtDebug.Text = currentFlashMode;
        //    });
        //}

        // Provide auto-focus in the viewfinder.
        private void focus_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (cam.IsFocusSupported == true)
            {
                //Focus when a capture is not in progress.
                try
                {
                    cam.Focus();
                }
                catch (Exception ex) { LittleWatson.ReportException(ex); }
            }
        }

        void cam_AutoFocusCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                try
                {
                    // Hide the focus brackets.
                    focusBrackets.Visibility = Visibility.Collapsed;

                    cam.CaptureImage();
                }
                catch (Exception ex) { LittleWatson.ReportException(ex); }
            });
        }

        // Provide touch focus in the viewfinder.
        void focus_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (cam != null)
            {
                if (cam.IsFocusAtPointSupported == true)
                {
                    try
                    {
                        // Determine location of tap.
                        Point tapLocation = e.GetPosition(viewfinderCanvas);

                        // Position focus brackets with estimated offsets.
                        focusBrackets.SetValue(Canvas.LeftProperty, tapLocation.X - 30);
                        focusBrackets.SetValue(Canvas.TopProperty, tapLocation.Y - 28);

                        // Determine focus point.
                        double focusXPercentage = tapLocation.X / viewfinderCanvas.Width;
                        double focusYPercentage = tapLocation.Y / viewfinderCanvas.Height;

                        // Show focus brackets and focus at point
                        focusBrackets.Visibility = Visibility.Visible;
                        cam.FocusAtPoint(focusXPercentage, focusYPercentage);

                    }
                    catch (Exception ex)
                    {
                        LittleWatson.ReportException(ex); 

                        // Cannot focus when a capture is in progress.
                        this.Dispatcher.BeginInvoke(delegate()
                        {
                            // Hide focus brackets.
                            focusBrackets.Visibility = Visibility.Collapsed;
                        });
                    }
                }
            }
        }

        // Provide auto-focus with a half button press using the hardware shutter button.
        private void OnButtonHalfPress(object sender, EventArgs e)
        {
            if (cam != null)
            {
                // Focus when a capture is not in progress.
                try
                {
                    //cam.Focus();
                }
                catch (Exception ex) { LittleWatson.ReportException(ex); }
            }
        }

        // Capture the image with a full button press using the hardware shutter button.
        private void OnButtonFullPress(object sender, EventArgs e)
        {
            if (cam != null)
            {
                //cam.Focus();
                try
                {
                    cam.CaptureImage();
                }
                catch (Exception ex) { LittleWatson.ReportException(ex); }
            }
        }

        // Cancel the focus if the half button press is released using the hardware shutter button.
        private void OnButtonRelease(object sender, EventArgs e)
        {

            if (cam != null)
            {
                try
                {
                    //cam.CancelFocus();
                }
                catch (Exception ex) { LittleWatson.ReportException(ex); }
            }
        }


    }
}