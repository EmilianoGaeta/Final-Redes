<?php 
require "config.php";
$ret = "";
$r = ExecuteQuery("Call 1practica.GetUser()");
while($row = mysqli_fetch_array($r, MYSQLI_BOTH))
{
	$ret = $ret . $row["user"] . "-";
}
echo($ret);

?>