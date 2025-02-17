﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Platformer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
		readonly PlatformerGame _game;

		public GamePage()
        {
            this.InitializeComponent();

			// Create the game.
			var launchArguments = string.Empty;
            _game = Microsoft.Xna.Platform.XamlGame<PlatformerGame>.Create(launchArguments, Window.Current.CoreWindow, swapChainPanel);
        }
    }
}
