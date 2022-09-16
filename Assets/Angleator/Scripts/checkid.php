<?php
$id = $_POST["id"];
$dateYEAR = date("Y");
$dateMONTH = date("n");
$dateDAY = date("j");

$myfile = file_get_contents("list.txt", "r");

if (strpos($myfile, $id) !== false) {
    //id is on file

    $pieces = explode(";", $myfile);

    foreach ($pieces as $value) {

      if (strpos($value, $id) !== false) 
      {
        $seperate = explode(",", $value);
        $installDate = $seperate[1];
        echo ("".$installDate.";".$dateYEAR." ".$dateMONTH." ".$dateDAY);
      }
    }
}else{
    //add to list
    $new = $id.",".$dateYEAR." ".$dateMONTH." ".$dateDAY.";";
    file_put_contents("list.txt", $new, FILE_APPEND | LOCK_EX);
    echo ("addition");
}

?>