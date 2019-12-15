<?php 
require "configPart2.php";
$r = ExecuteQuery("Call 1practica.QuitFriend('$u', '$p')");
DoEcho($r, 0);
?>