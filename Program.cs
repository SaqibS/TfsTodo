namespace TfsTodo
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Linq;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    internal static class Program
    {
        private static readonly WorkItemStore workItemStore = GetWorkItemStore();

        internal static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: MyTasks <command> [args]");
                return;
            }

            switch (args[0].ToLower())
            {
                case "list": List(); break;
                case "close": SetState(int.Parse(args[1]), "closed"); break;
                case "resolve": SetState(int.Parse(args[1]), "resolved"); break;
                case "show": Show(int.Parse(args[1])); break;
                default: break;
            }
        }

        private static WorkItemStore GetWorkItemStore()
        {
            string tfsUrl = ConfigurationManager.AppSettings["tfsUrl"];
            TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(new Uri(tfsUrl));
            WorkItemStore workItemStore = (WorkItemStore)tpc.GetService(typeof(WorkItemStore));
            return workItemStore;
        }

        private static void List()
        {
            string query = "SELECT  [System.Id], [System.WorkItemType], [System.Title], [System.AssignedTo], [System.State] FROM WorkItems WHERE [System.TeamProject] = 'Bing' AND [System.AssignedTo] = @me  AND  [System.State] <> 'Closed' ORDER BY [System.Id]";
            WorkItemCollection queryResults = workItemStore.Query(query);

            Console.WriteLine("{0} work items found", queryResults.Count);
            foreach (WorkItem wi in queryResults)
            {
                Console.WriteLine("{0}\t{1}\t{2}\tP{3}\t{4}", wi.Id, wi.Type.Name, wi.Title, wi.Fields["priority"].Value, wi.State);
            }
        }

        private static void SetState(int id, string state)
        {
            WorkItem wi = workItemStore.GetWorkItem(id);
            wi.State = state;
            ArrayList errors = wi.Validate();
            if (errors.Count > 0)
            {
                Console.WriteLine("Unable to save work item - errors found");
            }
            else
            {
                wi.Save();
            }
        }

        private static void Show(int id)
        {
            WorkItem wi = workItemStore.GetWorkItem(id);
            Console.WriteLine("{0} - {1} ({2} P{3} {4})", wi.Id, wi.Title, wi.State, wi.Fields["priority"].Value, wi.Type.Name);
            Console.WriteLine("Created by {0} on {1}", wi.CreatedBy, wi.CreatedDate.ToString());
            Console.WriteLine(wi.Description);
        }
    }
}
