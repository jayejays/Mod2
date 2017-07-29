
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace berry
{
    public class AzureManager
    {

        private static AzureManager i;
        private MobileServiceClient mob_client;
        private IMobileServiceTable<NotStrawberryModel> NotStrawberryTable;


        private AzureManager()
        {
            this.mob_client = new MobileServiceClient("http://strawberryornot.azurewebsites.net");
            this.NotStrawberryTable = this.mob_client.GetTable<NotStrawberryModel>();

        }

        public MobileServiceClient AzureClient
        {
            get { return mob_client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (i == null)
                {
                    i = new AzureManager();
                }

                return i;
            }
        }
        public async Task<List<NotStrawberryModel>> GetStrawberryInformation()
        {
            return await this.NotStrawberryTable.ToListAsync();
        }

        public async Task PostStrawberryInformation(NotStrawberryModel notStrawberryModel)
        {
            await this.NotStrawberryTable.InsertAsync(notStrawberryModel);
        }
    }
}