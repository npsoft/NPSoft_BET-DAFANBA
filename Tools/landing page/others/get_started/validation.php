<?php

/*--------------------------------------------------------------------------------------------*\

  validation.php
  --------------

  v2.3.3, Apr 2010

  This script provides generic validation for any web form. For a discussion and example usage 
  of this script, go to http://www.benjaminkeen.com/software/php_validation

  This script is written by Ben Keen with additional code contributed by Mihai Ionescu and 
  Nathan Howard. It is free to distribute, to re-write - to do what ever you want with it.

  Before using it, please read the following disclaimer. 

  THIS SOFTWARE IS PROVIDED ON AN "AS-IS" BASIS WITHOUT WARRANTY OF ANY KIND. BENJAMINKEEN.COM 
  SPECIFICALLY DISCLAIMS ANY OTHER WARRANTY, EXPRESS OR IMPLIED, INCLUDING ANY WARRANTY OF 
  MERCHANTABILITY OR FITNESS FOR A PARTICULAR PURPOSE. IN NO EVENT SHALL BENJAMINKEEN.COM BE 
  LIABLE FOR ANY CONSEQUENTIAL, INDIRECT, SPECIAL OR INCIDENTAL DAMAGES, EVEN IF BENJAMINKEEN.COM 
  HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH POTENTIAL LOSS OR DAMAGE. USER AGREES TO HOLD 
  BENJAMINKEEN.COM HARMLESS FROM AND AGAINST ANY AND ALL CLAIMS, LOSSES, LIABILITIES AND EXPENSES.

\*--------------------------------------------------------------------------------------------*/


/*--------------------------------------------------------------------------------------------*\
  Function: validateFields()
  Purpose:  generic form field validation.
  Parameters: field - the POST / GET fields from a form which need to be validated.
              rules - an array of the validation rules. Each rule is a string of the form:

   "[if:FIELDNAME=VALUE,]REQUIREMENT,fieldname[,fieldname2 [,fieldname3, date_flag]],error message"
  
              if:FIELDNAME=VALUE,   This allows us to only validate a field 
                          only if a fieldname FIELDNAME has a value VALUE. This 
                          option allows for nesting; i.e. you can have multiple 
                          if clauses, separated by a comma. They will be examined
                          in the order in which they appear in the line.

              Valid REQUIREMENT strings are: 
                "required"     - field must be filled in
                "digits_only"  - field must contain digits only
                "is_alpha"     - field must only contain alphanumeric characters (0-9, a-Z)
                "custom_alpha" - field must be of the custom format specified.
                      fieldname:  the name of the field
                      fieldname2: a character or sequence of special characters. These characters are:
                          L   An uppercase Letter.          V   An uppercase Vowel.
                          l   A lowercase letter.           v   A lowercase vowel.
                          D   A letter (upper or lower).    F   A vowel (upper or lower).
                          C   An uppercase Consonant.       x   Any number, 0-9.
                          c   A lowercase consonant.        X   Any number, 1-9.
                          E   A consonant (upper or lower).
                "reg_exp"      - field must match the supplied regular expression.  
                      fieldname:  the name of the field
                      fieldname2: the regular expression
                      fieldname3: (optional) flags for the reg exp (like i for case insensitive
                "letters_only" - field must only contains letters (a-Z)

                "length=X"     - field has to be X characters long
                "length=X-Y"   - field has to be between X and Y (inclusive) characters long
                "length>X"     - field has to be greater than X characters long
                "length>=X"    - field has to be greater than or equal to X characters long
                "length<X"     - field has to be less than X characters long
                "length<=X"    - field has to be less than or equal to X characters long

                "valid_email"  - field has to be valid email address
                "valid_date"   - field has to be a valid date
                      fieldname:  MONTH 
                      fieldname2: DAY 
                      fieldname3: YEAR
                      date_flag:  "later_date" / "any_date"
                "same_as"     - fieldname is the same as fieldname2 (for password comparison)

                "range=X-Y"    - field must be a number between the range of X and Y inclusive
                "range>X"      - field must be a number greater than X
                "range>=X"     - field must be a number greater than or equal to X
                "range<X"      - field must be a number less than X
                "range<=X"     - field must be a number less than or equal to X

  
  Comments:   With both digits_only, valid_email and is_alpha options, if the empty string is passed 
              in it won't generate an error, thus allowing validation of non-required fields. So,
              for example, if you want a field to be a valid email address, provide validation for 
              both "required" and "valid_email".
\*--------------------------------------------------------------------------------------------*/
function validateFields($fields, $rules)
{ 
  $errors = array();
  
  // loop through rules
  for ($i=0; $i<count($rules); $i++)
  {
    // split row into component parts 
    $row = explode(",", $rules[$i]);
    
    // while the row begins with "if:..." test the condition. If true, strip the if:..., part and 
    // continue evaluating the rest of the line. Keep repeating this while the line begins with an 
    // if-condition. If it fails any of the conditions, don't bother validating the rest of the line
    $satisfies_if_conditions = true;
    while (preg_match("/^if:/", $row[0]))
    {
      $condition = preg_replace("/^if:/", "", $row[0]);

      // check if it's a = or != test
      $comparison = "equal";
      $parts = array();
      if (preg_match("/!=/", $condition))
      {
        $parts = explode("!=", $condition);
        $comparison = "not_equal";
      }
      else 
        $parts = explode("=", $condition);

      $field_to_check = $parts[0];
      $value_to_check = $parts[1];
      
      // if the VALUE is NOT the same, we don't need to validate this field. Return.
      if ($comparison == "equal" && $fields[$field_to_check] != $value_to_check)
      {
        $satisfies_if_conditions = false;
        break;
      }
      else if ($comparison == "not_equal" && $fields[$field_to_check] == $value_to_check)
      {
        $satisfies_if_conditions = false;
        break;      
      }
      else 
        array_shift($row);    // remove this if-condition from line, and continue validating line
    }

    if (!$satisfies_if_conditions)
      continue;


    $requirement = $row[0];
    $field_name  = $row[1];

    // depending on the validation test, store the incoming strings for use later...
    if (count($row) == 6)        // valid_date
    {
      $field_name2   = $row[2];
      $field_name3   = $row[3];
      $date_flag     = $row[4];
      $error_message = $row[5];
    }
    else if (count($row) == 5)     // reg_exp (WITH flags like g, i, m)
    {
      $field_name2   = $row[2];
      $field_name3   = $row[3];
      $error_message = $row[4];
    }
    else if (count($row) == 4)     // same_as, custom_alpha, reg_exp (without flags like g, i, m)
    {
      $field_name2   = $row[2];
      $error_message = $row[3];
    }
    else
      $error_message = $row[2];    // everything else!


    // if the requirement is "length=...", rename requirement to "length" for switch statement
    if (preg_match("/^length/", $requirement))
    {
      $length_requirements = $requirement;
      $requirement         = "length";
    }

    // if the requirement is "range=...", rename requirement to "range" for switch statement
    if (preg_match("/^range/", $requirement))
    {
      $range_requirements = $requirement;
      $requirement        = "range";
    }


    // now, validate whatever is required of the field
    switch ($requirement)
    {
      case "required":
        if (!isset($fields[$field_name]) || $fields[$field_name] == "")
          $errors[] = $error_message;
        break;

      // doesn't fail if field is empty
      case "valid_email":
				$regexp="/^[a-z0-9]+([_+\\.-][a-z0-9]+)*@([a-z0-9]+([\.-][a-z0-9]+)*)+\\.[a-z]{2,}$/i";    
        if (isset($fields[$field_name]) && !empty($fields[$field_name]) && !preg_match($regexp, $fields[$field_name]))
          $errors[] = $error_message;
        break;

      
    }
  }
  
  return $errors;
}


?>
