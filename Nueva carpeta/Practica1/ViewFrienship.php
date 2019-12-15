<?php 
require "configPart2.php";
$r = ExecuteQuery("Call 1practica.ViewFrienship('$u', '$p')");
DoEcho($r, 0);
?>