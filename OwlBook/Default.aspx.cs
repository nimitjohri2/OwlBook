using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OwlBook
{

    //Programmed by Nimit Johri

    /// <summary>
    /// This application implements a friends suggestion system based on mutual friends
    /// The algorithm is based on the property of triadic closure where if there is are common nodes between two nodes then the common node tends to attract towards each other completing a triangle
    /// THe algorithm fetches the freinds of a node and for each friend node, fetches the friends of friends
    /// It maintains a counter for the number of times a friend of friend appeared which means that particular friend of friend has that many mutual friends with the host node
    /// 
    /// This application computes the one friend of friend which has the maximum mutual friends with the host node and adds them as friends
    /// The application selects a node at random and computes as above and adds a new friend
    /// Each function call does the above 100 times for acheiving analyzable results, which can be adjusted as desired
    /// 
    /// The application also calculated the average clustering coefficient of the graph by averaging the clustering coefficient of each node
    /// Clustering coefficient is the measure of the degree to which nodes in a graph tend to cluster together
    /// With each run, the algorithm should increase the clustering coefficient 
    /// The database also contains a table for clustering coefficient which stores the coefficient value after each run, where each run adds customizable number of friends, 100 at the time of testing
    /// As a proof of the algorithm working correctly, the clustering coefficient should rise after each run and also after each new friend made
    /// 
    /// This application as of now just computes the friends to be matched based on the number of mutual friends
    /// As it visits each friend of the user and each friend of firend of the user, it needs just slight modification to not just count number of occurrences of a friend of friend but number of friends of friends having a particular property
    /// For eaample, it can find the friend of firend who has the most mutual friends and has the most matching interests like music, sports, etc.
    /// It can suggest friends based on maximum number of matching interests like favourite sports team/player, favourite musicians, etc.
    /// 
    /// For this application, an undirected and unweighted graph has been used
    /// The graph has been stored in a database which contains two tables, users and edges
    /// The users table contains the details of the user
    /// The edges table contains the edges of the graph where an edge between two nodes is represents friendship
    /// </summary>

    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn_Run_Click(object sender, EventArgs e)
        {
                AddFriends();
                //CalculateClusteringCoefficient();
                //LoadChart();
        }

        //Function to add friends based on mutual friends
        private void AddFriends()
        {
            string connString = System.Configuration.ConfigurationManager.AppSettings["connstring"]; 
            string queryEdges = "Select * from edges";                                  //The graph i used contained 2000 nodes and 40000 edges which fit in memory
            string queryUsers = "Select * from users";                                  //For larger graph or for memory efficiency, these values can be not prefetched but whenevery required and only for the nodes in scope

            int friendsmade = 0;                                                        //Counter to run until the specified number of friends are made
            int k = 0;                                                                  //counter for indexing output array

            string[] output = new string[100];
            Array.Clear(output, 0, 100);

            DataTable edges = new DataTable();                                          //Datatable to store edges
            DataTable users = new DataTable();                                          //Datatable to store users

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();


                    SqlDataAdapter da = new SqlDataAdapter(queryEdges, conn);
                    da.Fill(edges);

                    da = new SqlDataAdapter(queryUsers, conn);
                    da.Fill(users);

                    while (friendsmade < 100)
                    {

                        Random rnd = new Random();
                        int person = rnd.Next(1, 2001);                                                                 //select a random person to find another user with most mutual friends
                        DataRow[] friends;
                        DataRow[] friendsoffriends;

                        Dictionary<int, int> potencialFriends = new Dictionary<int, int>();                             //Dictionary to store potencial friends

                        friends = edges.Select("A=" + person);                                                          //Find all the friends of the host node

                        foreach (DataRow row in friends)                                                                //Loop to process each friend of the host node
                        {
                            friendsoffriends = edges.Select("A=" + row.ItemArray[1]);                                   //Find all the friends of the friend of host node  

                            foreach (DataRow acquaintance in friendsoffriends)                                          //Each friend of friend will be called acquaintence
                            {
                                if ((edges.Select("A=" +person+ "AND B=" +acquaintance.ItemArray[1])).Length > 0)       //If the acquaintence is already a friend of the host node then continue
                                {
                                    continue;
                                }
                                if (potencialFriends.ContainsKey(Convert.ToInt32(acquaintance.ItemArray[1])))           //If the dictionary of potencial friends already contains this acquaintence then increment it's counter
                                {                                                                                       //This signifies that this acquaintence appeared again as a friend of another firend of the host node
                                    potencialFriends[Convert.ToInt32(acquaintance.ItemArray[1])]++;                     //In other words this counter suggests the number of mutual friends between the host node and this node
                                }
                                else
                                {
                                    potencialFriends.Add(Convert.ToInt32(acquaintance.ItemArray[1]), 1);                //If we came across this acquaintence for the first time then add it to the potencial friends dictionary with counter value 1
                                }
                            }
                        }
                        potencialFriends = potencialFriends.OrderByDescending(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);     //At the end of the loop, order the dictionary by the counter value to find the acquaintence node with most mutual friends


                        string queryNewEdges = "INSERT INTO EDGES VALUES (" + person + ", " + potencialFriends.First().Key + ")";       //Add the friendship edge between the acquaintence node and host node, which had the most mutual friends
                        SqlCommand command = new SqlCommand(queryNewEdges, conn);

                        try
                        {
                            friendsmade = friendsmade + command.ExecuteNonQuery();                                          //Increment the friends made counter
                            output[k++] = users.Rows[users.Rows.IndexOf(users.Select("id=" + person)[0])]["first_name"] + " is now friends with " + users.Rows[users.Rows.IndexOf(users.Select("id=" + potencialFriends.First().Key)[0])]["first_name"] + " and completed " + potencialFriends.First().Value + " triangles";
                            System.Diagnostics.Debug.WriteLine(friendsmade);
                        }
                        catch (Exception ex)
                        { }


                    }

                    lbl_friendsmade.Text = friendsmade + " new friends made";

                    gv_Display.DataSource = output;
                    gv_Display.DataBind();
                }
            }
            catch (Exception ex)
            {
            }
        }

        protected void btn_Clustering_Click(object sender, EventArgs e)
        {
            CalculateClusteringCoefficient();
        }

        //Method to calculate the average clustering coefficient of the graph by averaging the clustering coefficient of each node
        private void CalculateClusteringCoefficient()
        {
            string connString = System.Configuration.ConfigurationManager.AppSettings["connstring"];
            string queryEdges = "Select * from edges";
            string queryUsers = "Select * from users";
            string queryCoefficient = "Select * from clusteringcoefficient";

            DataTable edges = new DataTable();
            DataTable users = new DataTable();
            DataTable chart = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();


                    SqlDataAdapter da = new SqlDataAdapter(queryEdges, conn);
                    da.Fill(edges);

                    da = new SqlDataAdapter(queryUsers, conn);
                    da.Fill(users);

                    DataRow[] friends;
                    int maxEdges;
                    int common;
                    Dictionary<int, double> coefficient = new Dictionary<int, double>();                            //Dictionary for clustering coefficient

                    foreach (DataRow user in users.Rows)                                                            //Loop to process each node
                    {
                        common = 0;

                        friends = edges.Select("A=" + user.ItemArray[0]);                                           //Find all friends of this host user
                        maxEdges = friends.Length * (friends.Length - 1);                                           //Find maximum number of edges possible between this user's friends which is n(n-1) where n is the number of friends of this host user

                        foreach (DataRow friend in friends)                                                         //Loop to process each friend of this user
                        {
                            foreach (DataRow otherfriend in friends)                                                //Loop to process each mutual friend between the host user and this friend user
                            {
                                if (otherfriend.ItemArray[1].ToString() != friend.ItemArray[1].ToString())          //Check if it is the same friend in both firend and otherfriend
                                {
                                    DataRow[] frienship = edges.Select("A=" + friend.ItemArray[1] + " AND B=" + otherfriend.ItemArray[1]);      //Check if this friend is friends with another friend of the host node
                                    if (frienship.Length > 0)
                                        common++;                                                                   //If friendship found then increment counter
                                }
                            }
                        }
                        double ClusteringCoefficient = Convert.ToDouble(common.ToString() + ".0") / Convert.ToDouble(maxEdges.ToString() + ".0");       //Calculate the clustering coefficient for this host user by dividing the number of edges found between it's friends and the maximum possible edges

                        coefficient.Add(Convert.ToInt32(user.ItemArray[0]), ClusteringCoefficient);                 //Add clustering coefficient for this host node in dictionary


                    }

                    double avg = coefficient.Values.Sum() / coefficient.Count;                                      //Calculate the average of the clustering coefficent of all the clustering coefficient of all the nodes

                    string queryclustering = "INSERT INTO CLUSTERINGCOEFFICIENT VALUES (" + avg + ")";
                    SqlCommand command = new SqlCommand(queryclustering, conn);
                    command.ExecuteNonQuery();

                    lbl_clustering.Text = "The clustring coefficient is now " + avg;

                    //gv_Clustering.DataSource = avg;
                    //gv_Clustering.DataBind();

                    da = new SqlDataAdapter(queryCoefficient, conn);
                    da.Fill(chart);

                    //ch_clustering.DataBindTable(chart.AsEnumerable());

                }
            }
            catch (Exception ex)
            {


            }
        }

        protected void btn_loadchart_Click(object sender, EventArgs e)
        {
            LoadChart();
        }

        //This method is used to fetch the clustering coefficent values from the database and display a chart
        private void LoadChart()
        {
            string connString = System.Configuration.ConfigurationManager.AppSettings["connstring"];
            string queryCoefficient = "Select * from clusteringcoefficient";

            DataTable chart = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(queryCoefficient, conn);
                    da.Fill(chart);
                }

                ch_clustering.ChartAreas["ChartArea1"].AxisX.MajorGrid.Enabled = false;
                ch_clustering.ChartAreas["ChartArea1"].AxisY.MajorGrid.Enabled = false;

                ch_clustering.DataSource = chart;
                ch_clustering.Series["Series1"].YValueMembers = "Coefficient";
                ch_clustering.DataBind();
            }

            catch (Exception ex)
            { }

        }
    }
}