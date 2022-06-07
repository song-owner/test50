using Firebase.Database;
using Firebase.Database.Query;
using problemSolver.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace problemSolver
{
    public class FirebaseHelper
    {
        //FirebaseClient firebase = new FirebaseClient("https://fcmproj-f3461-default-rtdb.firebaseio.com//");
        FirebaseClient firebase = new FirebaseClient("https://clever-hangar-193006-default-rtdb.firebaseio.com/");
        public async Task<List<Problems>> GetAllProblems()
        {
            var tmp = new List<Problems>();
            try
            {
                tmp = (await firebase
                  .Child("Problems")
                  .OnceAsync<Problems>())
                  .Select(item => new Problems
                  {
                      PhoneId = item.Object.PhoneId,
                      SearchWord = item.Object.SearchWord,
                      HtmlTitle = item.Object.HtmlTitle,
                      Url = item.Object.Url,
                      Nice = item.Object.Nice,
                      Key = item.Key
                  }).ToList();
            }
            catch { }
            return tmp;
        }
        public async Task<List<Problems>> GetSearchProblems(string searchWord)
        {
            if (searchWord == "" || searchWord == null) return await GetAllProblems();
            var allProblems = await GetAllProblems();
            return allProblems.ToList();
        }
        public async Task AddProblem(string phonId, string search, string title, string url, string nice)
        {
            try {
                await firebase
                  .Child("Problems")
                  .PostAsync(new Problems() {
                      PhoneId = phonId, SearchWord = search, HtmlTitle = title, Url = url, Nice = nice });
            }
            catch { }
        }
        public async Task<Problems> GetProblem(string url)
        {
            var allProblems = await GetAllProblems();
            return allProblems.Where(a => a.Url == url).FirstOrDefault();
        }
        public async Task UpdatePerson(string key, string phonId, string search, string title, string url, string nice)
        {
            try {
                await firebase
                  .Child("Problems")
                  .Child(key)
                  .PutAsync(new Problems() {
                      PhoneId = phonId, SearchWord = search, HtmlTitle = title, Url = url, Nice = nice });
            }
            catch { }
        }
        public async Task DeleteProblem(string key)
        {
            await firebase.Child("Problems").Child(key).DeleteAsync();
        }
    }
}
