<?php

if(isset($_GET["user"])) {
    $u = $_GET["user"];
}

if(isset($_GET["pass"])) {
    $p = $_GET["pass"];
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