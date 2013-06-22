﻿<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="CoursePlayer_prototype.aspx.cs" Inherits="LMS_Prototype_1._CoursePlayer" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    
<script type="text/javascript">
    function NewHttpReq() {
        var httpReq = false;
        if (typeof XMLHttpRequest != 'undefined') {
            httpReq = new XMLHttpRequest();
        } else {
            try {
                httpReq = new ActiveXObject("Msxml2.XMLHTTP.4.0");
            } catch (e) {
                try {
                    httpReq = new ActiveXObject("Msxml2.XMLHTTP");
                } catch (ee) {
                    try {
                        httpReq = new ActiveXObject("Microsoft.XMLHTTP");
                    } catch (eee) {
                        httpReq = false;
                    }
                }
            }
        }
        return httpReq;
    }

    function DoRequest(httpReq, url, param) {

        // httpReq.open (Method("get","post"), URL(string), Asyncronous(true,false))
        //popupwin(url+"\n"+param);
        httpReq.open("POST", url, false);
        httpReq.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        httpReq.send(param);
        if (httpReq.status == 200) {
            //popupwin(url+"\n"+param+"\n"+httpReq.responseText);
            return httpReq.responseText;
        } else {
            return httpReq.status;
        }
    }

</script>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

<%

    // ASP.NET course player incorporating AICC-SCORM API JS
    /*
if (isset($userdata->status)) {
    if ($userdata->status == '') {
        $userdata->entry = 'ab-initio';
    } else {
        if (isset($userdata->{'cmi.core.exit'}) && ($userdata->{'cmi.core.exit'} == 'suspend')) {
            $userdata->entry = 'resume';
        } else {
            $userdata->entry = '';
        }
    }
}
if (!isset($currentorg)) {
    $currentorg = '';
}
   */
    
%>

<script type="text/javascript" >
//
// AICC-SCORM API Implementation
//
    function AICCapi() {
        // Standard Data Types
        CMIString256 = '^.{0,255}$';
        CMIString4096 = '^.{0,4096}$';
        CMITime = '^([0-2]{1}[0-9]{1}):([0-5]{1}[0-9]{1}):([0-5]{1}[0-9]{1})(\.[0-9]{1,2})?$';
        CMITimespan = '^([0-9]{2,4}):([0-9]{2}):([0-9]{2})(\.[0-9]{1,2})?$';
        CMIInteger = '^\\d+$';
        CMISInteger = '^-?([0-9]+)$';
        CMIDecimal = '^-?([0-9]{0,3})(\.[0-9]{1,2})?$';
        CMIIdentifier = '^\\w{1,255}$';
        CMIFeedback = CMIString256;
        CMIIndex = '[._](\\d+).';

        // Vocabulary Data Types
        CMIStatus = '^passed$|^completed$|^failed$|^incomplete$|^browsed$';
        CMIStatus2 = '^passed$|^completed$|^failed$|^incomplete$|^browsed$|^not attempted$';
        CMIExit = '^time-out$|^suspend$|^logout$|^$';
        CMIType = '^true-false$|^choice$|^fill-in$|^matching$|^performance$|^sequencing$|^likert$|^numeric$';
        CMIResult = '^correct$|^wrong$|^unanticipated$|^neutral$|^([0-9]{0,3})?(\.[0-9]{1,2})?$';
        NAVEvent = '^previous$|^continue$';

        // Children lists
        cmi_children = 'core, suspend_data, launch_data, comments, objectives, student_data, student_preference, interactions';
        core_children = 'student_id, student_name, lesson_location, credit, lesson_status, entry, score, total_time, lesson_mode, exit, session_time';
        score_children = 'raw, min, max';
        comments_children = 'content, location, time';
        objectives_children = 'id, score, status';
        student_data_children = 'attempt_number, tries, mastery_score, max_time_allowed, time_limit_action';
        student_preference_children = 'audio, language, speed, text';
        interactions_children = 'id, objectives, time, type, correct_responses, weighting, student_response, result, latency';

        // Data ranges
        score_range = '0#100';
        audio_range = '-1#100';
        speed_range = '-100#100';
        weighting_range = '-100#100';
        text_range = '-1#1';

        // SCORM-AICC data model
        var datamodel =  {
            'cmi._children':{'defaultvalue':cmi_children, 'mod':'r', 'writeerror':'402'},
            'cmi._version':{'defaultvalue':'3.4', 'mod':'r', 'writeerror':'402'},
            'cmi.core._children':{'defaultvalue':core_children, 'mod':'r', 'writeerror':'402'},
            'cmi.core.student_id':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'403'},
            'cmi.core.student_name':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'403'},
            'cmi.core.lesson_location':{'defaultvalue':'<% %>', 'format':CMIString256, 'mod':'rw', 'writeerror':'405'},
            'cmi.core.credit':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'403'},
            'cmi.core.lesson_status':{'defaultvalue':'<% %>', 'format':CMIStatus, 'mod':'rw', 'writeerror':'405'},
            'cmi.core.exit':{'defaultvalue':'<% %>', 'format':CMIExit, 'mod':'w', 'readerror':'404', 'writeerror':'405'},
            'cmi.core.entry':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'403'},
            'cmi.core.score._children':{'defaultvalue':score_children, 'mod':'r', 'writeerror':'402'},
            'cmi.core.score.raw':{'defaultvalue':'<% %>', 'format':CMIDecimal, 'range':score_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.core.score.max':{'defaultvalue':'<% %>', 'format':CMIDecimal, 'range':score_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.core.score.min':{'defaultvalue':'<% %>', 'format':CMIDecimal, 'range':score_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.core.session_time':{'format':CMITimespan, 'mod':'w', 'defaultvalue':'00:00:00', 'readerror':'404', 'writeerror':'405'},
            'cmi.core.total_time':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'403'},
            'cmi.core.lesson_mode':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'403'},
            'cmi.suspend_data':{'defaultvalue':'<% %>', 'format':CMIString4096, 'mod':'rw', 'writeerror':'405'},
            'cmi.launch_data':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'403'},
            'cmi.comments':{'defaultvalue':'<% %>', 'format':CMIString4096, 'mod':'rw', 'writeerror':'405'},
            // deprecated evaluation attributes
            'cmi.evaluation.comments._count':{'defaultvalue':'0', 'mod':'r', 'writeerror':'402'},
            'cmi.evaluation.comments._children':{'defaultvalue':comments_children, 'mod':'r', 'writeerror':'402'},
            'cmi.evaluation.comments.n.content':{'defaultvalue':'', 'pattern':CMIIndex, 'format':CMIString256, 'mod':'rw', 'writeerror':'405'},
            'cmi.evaluation.comments.n.location':{'defaultvalue':'', 'pattern':CMIIndex, 'format':CMIString256, 'mod':'rw', 'writeerror':'405'},
            'cmi.evaluation.comments.n.time':{'defaultvalue':'', 'pattern':CMIIndex, 'format':CMITime, 'mod':'rw', 'writeerror':'405'},
            'cmi.comments_from_lms':{'mod':'r', 'writeerror':'403'},
            'cmi.objectives._children':{'defaultvalue':objectives_children, 'mod':'r', 'writeerror':'402'},
            'cmi.objectives._count':{'mod':'r', 'defaultvalue':'0', 'writeerror':'402'},
            'cmi.objectives.n.id':{'pattern':CMIIndex, 'format':CMIIdentifier, 'mod':'rw', 'writeerror':'405'},
            'cmi.objectives.n.score._children':{'pattern':CMIIndex, 'mod':'r', 'writeerror':'402'},
            'cmi.objectives.n.score.raw':{'defaultvalue':'', 'pattern':CMIIndex, 'format':CMIDecimal, 'range':score_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.objectives.n.score.min':{'defaultvalue':'', 'pattern':CMIIndex, 'format':CMIDecimal, 'range':score_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.objectives.n.score.max':{'defaultvalue':'', 'pattern':CMIIndex, 'format':CMIDecimal, 'range':score_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.objectives.n.status':{'pattern':CMIIndex, 'format':CMIStatus2, 'mod':'rw', 'writeerror':'405'},
            'cmi.student_data._children':{'defaultvalue':student_data_children, 'mod':'r', 'writeerror':'402'},
            'cmi.student_data.attempt_number':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'402'},
            'cmi.student_data.tries.n.score.raw':{'defaultvalue':'', 'pattern':CMIIndex, 'format':CMIDecimal, 'range':score_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.student_data.tries.n.score.min':{'defaultvalue':'', 'pattern':CMIIndex, 'format':CMIDecimal, 'range':score_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.student_data.tries.n.score.max':{'defaultvalue':'', 'pattern':CMIIndex, 'format':CMIDecimal, 'range':score_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.student_data.tries.n.status':{'pattern':CMIIndex, 'format':CMIStatus2, 'mod':'rw', 'writeerror':'405'},
            'cmi.student_data.tries.n.time':{'pattern':CMIIndex, 'format':CMITime, 'mod':'rw', 'writeerror':'405'},
            'cmi.student_data.mastery_score':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'403'},
            'cmi.student_data.max_time_allowed':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'403'},
            'cmi.student_data.time_limit_action':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'403'},
            'cmi.student_data.tries_during_lesson':{'defaultvalue':'<% %>', 'mod':'r', 'writeerror':'402'},
            'cmi.student_preference._children':{'defaultvalue':student_preference_children, 'mod':'r', 'writeerror':'402'},
            'cmi.student_preference.audio':{'defaultvalue':'0', 'format':CMISInteger, 'range':audio_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.student_preference.language':{'defaultvalue':'', 'format':CMIString256, 'mod':'rw', 'writeerror':'405'},
            'cmi.student_preference.speed':{'defaultvalue':'0', 'format':CMISInteger, 'range':speed_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.student_preference.text':{'defaultvalue':'0', 'format':CMISInteger, 'range':text_range, 'mod':'rw', 'writeerror':'405'},
            'cmi.interactions._children':{'defaultvalue':interactions_children, 'mod':'r', 'writeerror':'402'},
            'cmi.interactions._count':{'mod':'r', 'defaultvalue':'0', 'writeerror':'402'},
            'cmi.interactions.n.id':{'pattern':CMIIndex, 'format':CMIIdentifier, 'mod':'w', 'readerror':'404', 'writeerror':'405'},
            'cmi.interactions.n.objectives._count':{'pattern':CMIIndex, 'mod':'r', 'defaultvalue':'0', 'writeerror':'402'},
            'cmi.interactions.n.objectives.n.id':{'pattern':CMIIndex, 'format':CMIIdentifier, 'mod':'w', 'readerror':'404', 'writeerror':'405'},
            'cmi.interactions.n.time':{'pattern':CMIIndex, 'format':CMITime, 'mod':'w', 'readerror':'404', 'writeerror':'405'},
            'cmi.interactions.n.type':{'pattern':CMIIndex, 'format':CMIType, 'mod':'w', 'readerror':'404', 'writeerror':'405'},
            'cmi.interactions.n.correct_responses._count':{'pattern':CMIIndex, 'mod':'r', 'defaultvalue':'0', 'writeerror':'402'},
            'cmi.interactions.n.correct_responses.n.pattern':{'pattern':CMIIndex, 'format':CMIFeedback, 'mod':'w', 'readerror':'404', 'writeerror':'405'},
            'cmi.interactions.n.weighting':{'pattern':CMIIndex, 'format':CMIDecimal, 'range':weighting_range, 'mod':'w', 'readerror':'404', 'writeerror':'405'},
            'cmi.interactions.n.student_response':{'pattern':CMIIndex, 'format':CMIFeedback, 'mod':'w', 'readerror':'404', 'writeerror':'405'},
            'cmi.interactions.n.result':{'pattern':CMIIndex, 'format':CMIResult, 'mod':'w', 'readerror':'404', 'writeerror':'405'},
            'cmi.interactions.n.latency':{'pattern':CMIIndex, 'format':CMITimespan, 'mod':'w', 'readerror':'404', 'writeerror':'405'},
            'nav.event':{'defaultvalue':'', 'format':NAVEvent, 'mod':'w', 'readerror':'404', 'writeerror':'405'}
        };

	
        //
        // Datamodel initialization
        //
        var cmi = new Object();
        cmi.core = new Object();
        cmi.core.score = new Object();
        cmi.objectives = new Object();
        cmi.student_data = new Object();
        cmi.student_preference = new Object();
        cmi.interactions = new Object();
        cmi.evaluation = new Object();
        cmi.evaluation.comments = new Object();

        // Navigation Object
        var nav = new Object();

        for (element in datamodel) {
            if (element.match(/\.n\./) == null) {
                if ((typeof eval('datamodel["'+element+'"].defaultvalue')) != 'undefined') {
                    eval(element+' = datamodel["'+element+'"].defaultvalue;');
                } else {
                    eval(element+' = "";');
                }
            }
        }

        <%
/*
$current_objective = '';
$count = 0;
$objectives = '';
foreach ($userdata as $element => $value) {
    if (substr($element, 0, 14) == 'cmi.objectives') {
        $element = preg_replace('/\.(\d+)\./', "_\$1.", $element);
        preg_match('/\_(\d+)\./', $element, $matches);
        if (count($matches) > 0 && $current_objective != $matches[1]) {
            $current_objective = $matches[1];
            $count++;
            $end = strpos($element, $matches[1])+strlen($matches[1]);
            $subelement = substr($element, 0, $end);
            echo '    '.$subelement." = new Object();\n";
            echo '    '.$subelement.".score = new Object();\n";
            echo '    '.$subelement.".score._children = score_children;\n";
            echo '    '.$subelement.".score.raw = '';\n";
            echo '    '.$subelement.".score.min = '';\n";
            echo '    '.$subelement.".score.max = '';\n";
        }
        echo '    '.$element.' = \''.$value."';\n";
    }
}
if ($count > 0) {
    echo '    cmi.objectives._count = '.$count.";\n";
}
*/

%>

        if (cmi.core.lesson_status == '') {
            cmi.core.lesson_status = 'not attempted';
        }

        //
        // API Methods definition
        //
        var Initialized = false;

        function LMSInitialize (param) {
            errorCode = "0";
            if (param == "") {
                if (!Initialized) {
                    Initialized = true;
                    errorCode = "0";
                    return "true";
                } else {
                    errorCode = "101";
                }
            } else {
                errorCode = "201";
            }
            return "false";
        }

        function LMSFinish (param) {
            errorCode = "0";
            if (param == "") {
                if (Initialized) {
                    Initialized = false;
                    result = StoreData(cmi,true);
                    if (nav.event != '') {
                        if (nav.event == 'continue') {
                            setTimeout('mod_scorm_launch_next_sco();',500);
                        } else {
                            setTimeout('mod_scorm_launch_prev_sco();',500);
                        }
                    } else {
                        if (<%="2" %> == 1) {
                            setTimeout('mod_scorm_launch_next_sco();',500);
                        }
                    }
                    // trigger TOC update
                    var sURL = "<% %>";
                    var callback = M.mod_scorm.connectPrereqCallback;
                    YUI().use('yui2-connection', function(Y) {
                        Y.YUI2.util.Connect.asyncRequest('GET', sURL, callback, null);
                    });
                    return "true";
                } else {
                    errorCode = "301";
                }
            } else {
                errorCode = "201";
            }
            return "false";
        }

        function LMSGetValue (element) {
            errorCode = "0";
            if (Initialized) {
                if (element !="") {
                    expression = new RegExp(CMIIndex,'g');
                    elementmodel = String(element).replace(expression,'.n.');
                    if ((typeof eval('datamodel["'+elementmodel+'"]')) != "undefined") {
                        if (eval('datamodel["'+elementmodel+'"].mod') != 'w') {
                            element = String(element).replace(expression, "_$1.");
                            elementIndexes = element.split('.');
                            subelement = 'cmi';
                            i = 1;
                            while ((i < elementIndexes.length) && (typeof eval(subelement) != "undefined")) {
                                subelement += '.'+elementIndexes[i++];
                            }
                            if (subelement == element) {
                                errorCode = "0";
                                return eval(element);
                            } else {
                                errorCode = "0"; // Need to check if it is the right errorCode
                            }
                        } else {
                            errorCode = eval('datamodel["'+elementmodel+'"].readerror');
                        }
                    } else {
                        childrenstr = '._children';
                        countstr = '._count';
                        if (elementmodel.substr(elementmodel.length-childrenstr.length,elementmodel.length) == childrenstr) {
                            parentmodel = elementmodel.substr(0,elementmodel.length-childrenstr.length);
                            if ((typeof eval('datamodel["'+parentmodel+'"]')) != "undefined") {
                                errorCode = "202";
                            } else {
                                errorCode = "201";
                            }
                        } else if (elementmodel.substr(elementmodel.length-countstr.length,elementmodel.length) == countstr) {
                            parentmodel = elementmodel.substr(0,elementmodel.length-countstr.length);
                            if ((typeof eval('datamodel["'+parentmodel+'"]')) != "undefined") {
                                errorCode = "203";
                            } else {
                                errorCode = "201";
                            }
                        } else {
                            errorCode = "201";
                        }
                    }
                } else {
                    errorCode = "201";
                }
            } else {
                errorCode = "301";
            }
            return "";
        }

        function LMSSetValue (element,value) {
            errorCode = "0";
            if (Initialized) {
                if (element != "") {
                    expression = new RegExp(CMIIndex,'g');
                    elementmodel = String(element).replace(expression,'.n.');
                    if ((typeof eval('datamodel["'+elementmodel+'"]')) != "undefined") {
                        if (eval('datamodel["'+elementmodel+'"].mod') != 'r') {
                            expression = new RegExp(eval('datamodel["'+elementmodel+'"].format'));
                            value = value+'';
                            matches = value.match(expression);
                            if (matches != null) {
                                //Create dynamic data model element
                                if (element != elementmodel) {
                                    elementIndexes = element.split('.');
                                    subelement = 'cmi';
                                    for (i=1;i < elementIndexes.length-1;i++) {
                                        elementIndex = elementIndexes[i];
                                        if (elementIndexes[i+1].match(/^\d+$/)) {
                                            if ((typeof eval(subelement+'.'+elementIndex)) == "undefined") {
                                                eval(subelement+'.'+elementIndex+' = new Object();');
                                                eval(subelement+'.'+elementIndex+'._count = 0;');
                                            }
                                            if (elementIndexes[i+1] == eval(subelement+'.'+elementIndex+'._count')) {
                                                eval(subelement+'.'+elementIndex+'._count++;');
                                            }
                                            if (elementIndexes[i+1] > eval(subelement+'.'+elementIndex+'._count')) {
                                                errorCode = "201";
                                            }
                                            subelement = subelement.concat('.'+elementIndex+'_'+elementIndexes[i+1]);  // 'cmi.interactions_n.[element]'
                                            i++;
                                        } else {
                                            subelement = subelement.concat('.'+elementIndex);
                                        }
                                        if ((typeof eval(subelement)) == "undefined") {
                                            eval(subelement+' = new Object();');
                                            if (subelement.substr(0,14) == 'cmi.objectives') {
                                                eval(subelement+'.score = new Object();');
                                                eval(subelement+'.score._children = score_children;');
                                                eval(subelement+'.score.raw = "";');
                                                eval(subelement+'.score.min = "";');
                                                eval(subelement+'.score.max = "";');
                                            }
                                            if (subelement.substr(0,16) == 'cmi.interactions') {
                                                eval(subelement+'.objectives = new Object();');
                                                eval(subelement+'.objectives._count = 0;');
                                                eval(subelement+'.correct_responses = new Object();');
                                                eval(subelement+'.correct_responses._count = 0;');
                                            }
                                        }
                                    }
                                    element = subelement.concat('.'+elementIndexes[elementIndexes.length-1]);
                                }
                                //Store data
                                if (errorCode == "0") {
                                    if ((typeof eval('datamodel["'+elementmodel+'"].range')) != "undefined") {
                                        range = eval('datamodel["'+elementmodel+'"].range');
                                        ranges = range.split('#');
                                        value = value*1.0;
                                        if ((value >= ranges[0]) && (value <= ranges[1])) {
                                            eval(element+'="'+value+'";');
                                            errorCode = "0";
                                            return "true";
                                        } else {
                                            errorCode = eval('datamodel["'+elementmodel+'"].writeerror');
                                        }
                                    } else {
                                        if (element == 'cmi.comments') {
                                            eval(element+'+="'+value+'";');
                                        } else {
                                            eval(element+'="'+value+'";');
                                        }
                                        errorCode = "0";
                                        return "true";
                                    }
                                }
                            } else {
                                errorCode = eval('datamodel["'+elementmodel+'"].writeerror');
                            }
                        } else {
                            errorCode = eval('datamodel["'+elementmodel+'"].writeerror');
                        }
                    } else {
                        errorCode = "201"
                    }
                } else {
                    errorCode = "201";
                }
            } else {
                errorCode = "301";
            }
            return "false";
        }

        function LMSCommit (param) {
            errorCode = "0";
            if (param == "") {
                if (Initialized) {
                    result = StoreData(cmi,false);
                    return "true";
                } else {
                    errorCode = "301";
                }
            } else {
                errorCode = "201";
            }
            return "false";
        }

        function LMSGetLastError () {
            return errorCode;
        }

        function LMSGetErrorString (param) {
            if (param != "") {
                var errorString = new Array();
                errorString["0"] = "No error";
                errorString["101"] = "General exception";
                errorString["201"] = "Invalid argument error";
                errorString["202"] = "Element cannot have children";
                errorString["203"] = "Element not an array - cannot have count";
                errorString["301"] = "Not initialized";
                errorString["401"] = "Not implemented error";
                errorString["402"] = "Invalid set value, element is a keyword";
                errorString["403"] = "Element is read only";
                errorString["404"] = "Element is write only";
                errorString["405"] = "Incorrect data type";
                return errorString[param];
            } else {
                return "";
            }
        }

        function LMSGetDiagnostic (param) {
            if (param == "") {
                param = errorCode;
            }
            return param;
        }

        function AddTime (first, second) {
            var sFirst = first.split(":");
            var sSecond = second.split(":");
            var cFirst = sFirst[2].split(".");
            var cSecond = sSecond[2].split(".");
            var change = 0;

            FirstCents = 0;  //Cents
            if (cFirst.length > 1) {
                FirstCents = parseInt(cFirst[1],10);
            }
            SecondCents = 0;
            if (cSecond.length > 1) {
                SecondCents = parseInt(cSecond[1],10);
            }
            var cents = FirstCents + SecondCents;
            change = Math.floor(cents / 100);
            cents = cents - (change * 100);
            if (Math.floor(cents) < 10) {
                cents = "0" + cents.toString();
            }

            var secs = parseInt(cFirst[0],10)+parseInt(cSecond[0],10)+change;  //Seconds
            change = Math.floor(secs / 60);
            secs = secs - (change * 60);
            if (Math.floor(secs) < 10) {
                secs = "0" + secs.toString();
            }

            mins = parseInt(sFirst[1],10)+parseInt(sSecond[1],10)+change;   //Minutes
            change = Math.floor(mins / 60);
            mins = mins - (change * 60);
            if (mins < 10) {
                mins = "0" + mins.toString();
            }

            hours = parseInt(sFirst[0],10)+parseInt(sSecond[0],10)+change;  //Hours
            if (hours < 10) {
                hours = "0" + hours.toString();
            }

            if (cents != '0') {
                return hours + ":" + mins + ":" + secs + '.' + cents;
            } else {
                return hours + ":" + mins + ":" + secs;
            }
        }

        function TotalTime() {
            total_time = AddTime(cmi.core.total_time, cmi.core.session_time);
            return '&'+underscore('cmi.core.total_time')+'='+escape(total_time);
        }

        function CollectData(data,parent) {
            var datastring = '';
            for (property in data) {
                if (typeof data[property] == 'object') {
                    datastring += CollectData(data[property],parent+'.'+property);
                } else {
                    element = parent+'.'+property;
                    expression = new RegExp(CMIIndex,'g');
                    elementmodel = String(element).replace(expression,'.n.');
                    if ((typeof eval('datamodel["'+elementmodel+'"]')) != "undefined") {
                        if (eval('datamodel["'+elementmodel+'"].mod') != 'r') {
                            elementstring = '&'+underscore(element)+'='+escape(data[property]);
                            if ((typeof eval('datamodel["'+elementmodel+'"].defaultvalue')) != "undefined") {
                                if (eval('datamodel["'+elementmodel+'"].defaultvalue') != data[property]) {
                                    datastring += elementstring;
                                }
                            } else {
                                datastring += elementstring;
                            }
                        }
                    }
                }
            }
            return datastring;
        }

        function StoreData(data,storetotaltime) {
            if (storetotaltime) {
                if (cmi.core.lesson_mode == 'normal') {
                    if (cmi.core.credit == 'credit') {
                        if (cmi.student_data.mastery_score != '' && cmi.core.score.raw != '') {
                            if (cmi.core.score.raw >= cmi.student_data.mastery_score) {
                                cmi.core.lesson_status = 'passed';
                            } else {
                                cmi.core.lesson_status = 'failed';
                            }
                        }
                    }
                }
                if (cmi.core.lesson_mode == 'browse') {
                    if (datamodel['cmi.core.lesson_status'].defaultvalue == '' && cmi.core.lesson_status == 'not attempted') {
                        cmi.core.lesson_status = 'browsed';
                    }
                }
                datastring = CollectData(data,'cmi');
                datastring += TotalTime();
            } else {
                datastring = CollectData(data,'cmi');
            }
            datastring += '&attempt=<% %>';
            datastring += '&scoid=<% %>';

            //popupwin(datastring);
            var myRequest = NewHttpReq();
            result = DoRequest(myRequest,"<% %>"+datastring);
            results = String(result).split('\n');
            errorCode = results[1];
            return results[0];
        }

        this.LMSInitialize = LMSInitialize;
        this.LMSFinish = LMSFinish;
        this.LMSGetValue = LMSGetValue;
        this.LMSSetValue = LMSSetValue;
        this.LMSCommit = LMSCommit;
        this.LMSGetLastError = LMSGetLastError;
        this.LMSGetErrorString = LMSGetErrorString;
        this.LMSGetDiagnostic = LMSGetDiagnostic;
        
        this.cmi = cmi;
    }

    var API = new AICCapi();

</script>

<iframe id="toc" style="visibility: hidden; display: none" ></iframe>
<iframe id="content" src="/Courses/SCORM-API_Test/QUnit-Tests/qunit_SCORM_API.html" 
    style="width:900px; height:700px;"></iframe>
    <!--
    
    /Courses/RuntimeBasicCalls_SCORM12/shared/launchpage.html
    /Courses/AICC_SCORM_Course_Example/sco_01.html
    /Courses/SCORM-API_Test/QUnit-Tests/qunit_SCORM_API.html  - qunit_SCOBot_basic.html - qunit_SCOBot.html

    -->
    <p>
    </p>

<iframe id="cmiresults" style="width:800px; height:50px;"></iframe>

</asp:Content>