using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace MicrophoneEchoSample
{
    [Activity(Label = "MicrophoneEchoSample"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.UiMode | ConfigChanges.SmallestScreenSize
        , ScreenOrientation = ScreenOrientation.FullSensor
    )]
    public class MicrophoneEchoSampleActivity : Microsoft.Xna.Framework.AndroidGameActivity
    {
        const int RequestMicrophoneId = 10;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Request microphone permission
            if (CheckSelfPermission(Manifest.Permission.RecordAudio) != Permission.Granted)
                RequestPermissions(new[] { Manifest.Permission.RecordAudio }, RequestMicrophoneId);
            else
                Android.Widget.Toast.MakeText(this, "Microphone permission already granted!", Android.Widget.ToastLength.Short).Show();


            var game = new MicrophoneEchoSampleGame();
            SetContentView((View)game.Services.GetService(typeof(View)));
            game.Run();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RequestMicrophoneId)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    Android.Widget.Toast.MakeText(this, "Microphone permission granted!", Android.Widget.ToastLength.Short).Show();
                else
                    Android.Widget.Toast.MakeText(this, "Microphone permission denied!", Android.Widget.ToastLength.Short).Show();
            }
        }

    }
}

