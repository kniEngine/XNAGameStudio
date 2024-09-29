using System.ComponentModel;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace Primitives3D
{
    [Activity(Label = "Primitives3D"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.UiMode | ConfigChanges.SmallestScreenSize
                               | ConfigChanges.Density | ConfigChanges.LayoutDirection | ConfigChanges.FontScale
        , ScreenOrientation = ScreenOrientation.Landscape
        , ExcludeFromRecents = true
        , Exported = true
    )]
    [IntentFilter(new[] { Android.Content.Intent.ActionMain },
     Categories = new[]
     {
       "org.khronos.openxr.intent.category.IMMERSIVE_HMD",
       "com.oculus.intent.category.VR",
       "android.intent.category.LAUNCHER"
     }
    )]
    public class Primitives3DActivity : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var game = new Primitives3DVRGame();
            SetContentView((View)game.Services.GetService(typeof(View)));
            game.Run();
        }
    }
}

