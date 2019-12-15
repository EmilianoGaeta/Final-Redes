<?php 
require "configPart2.php";
$r = ExecuteQuery("Call 1practica.AddFriend('$u', '$p')");
DoEcho($r, 0);
?>