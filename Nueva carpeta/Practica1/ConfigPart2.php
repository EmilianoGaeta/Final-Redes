<?php

if(isset($_GET["user1"])) {
    $u = $_GET["user1"];
}

if(isset($_GET["user2"])) {
    $p = $_GET["user2"];
}

function ExecuteQuery($q)
{
	$con = mysqli_connect("localhost","root","","");
	$eq = mysqli_query($con, $q);
	mysqli_close($con);
	return $eq;
}

function DoEcho($r, $n)
{
	echo(mysqli_fetch_array($r, MYSQLI_BOTH)[$n]);
}

?>