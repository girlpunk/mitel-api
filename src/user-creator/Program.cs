using System.Text;
using MySql.Data.MySqlClient;
using mitelapi;
using mitelapi.Types;

using var sql = new MySqlConnection("server=x.x.x.x;uid=xxx;pwd=xxx;database=asterisk");
sql.Open();

// Find the current highest ID
var countQuery = new MySqlCommand($"select max(id) from asterisk.ps_endpoints;");
countQuery.Connection = sql;
var countQueryReader = countQuery.ExecuteReader();
countQueryReader.Read();

// Increment that ID and use it as the next extension number
var number = (countQueryReader.GetInt32(0) + 1).ToString();
countQueryReader.Close();
Console.WriteLine($"Number: {number}");

// Generate a PIN
var random = new Random();
var pin = random.Next(0, 9999).ToString("D4");
Console.WriteLine($"PIN: {pin}");

// Use the CLI parameter as a name
var name = args[0];

// Generate a SIP password
var sip = Guid.NewGuid().ToString("d").Substring(0, 10);


// Connect to Mitel OMM
var client = new OmmClient("x.x.x.x");
await client.LoginAsync("xxx", "xxx");

var cancellationTokenSource = new CancellationTokenSource();

// Create a user
var PPuser = new PPUserType()
{
    Name = name,
    Num = number,
    Pin = pin,
    SipAuthId = number,
    SipPw = sip,
    AddId = number,
};
var user = await client.CreatePPUserAsync(PPuser, cancellationTokenSource.Token);


// Add endpoint details to Mitel
var query1 = new MySqlCommand($"insert into asterisk.ps_endpoints (id,transport,aors,auth,context,disallow,allow) values ('{number}','transport-udp','{number}','{number}','sets','all','ulaw,alaw');");
var query2 = new MySqlCommand($"insert into asterisk.ps_aors (id,max_contacts) values ('{number}',2);");
var query3 = new MySqlCommand($"insert into asterisk.ps_auths (id,auth_type,password,username) values ('{number}', 'userpass', '{sip}', '{number}');");

query1.Connection = sql;
query1.ExecuteNonQuery();

query2.Connection = sql;
query2.ExecuteNonQuery();

query3.Connection = sql;
query3.ExecuteNonQuery();


sql.Close();
