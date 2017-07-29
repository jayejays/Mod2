using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;

namespace berry
{
    public partial class AzureTable : ContentPage
    {

        public AzureTable()
        {
            InitializeComponent();

        }

        async void Handle_ClickedAsync(object sender, System.EventArgs e)
        {
            List<NotStrawberryModel> NotStrawberryInformation = await AzureManager.AzureManagerInstance.GetStrawberryInformation();
            StrawberryList.ItemsSource = NotStrawberryInformation;

        }

    }
}