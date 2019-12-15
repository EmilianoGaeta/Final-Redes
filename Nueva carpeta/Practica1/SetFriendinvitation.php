<?php 
require "configPart2.php";
$r = ExecuteQuery("Call 1practica.SetFrienInvitation('$u', '$p' , '$s')");
DoEcho($r, 0);
?>