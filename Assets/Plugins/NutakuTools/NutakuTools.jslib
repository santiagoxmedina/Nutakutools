var LibraryJsCallCsTest = {
	  	$JsCallCsTest: {},
	  	CheckGuestPlayer: function (action) {
	    	var params = {};
			params[opensocial.DataRequest.PeopleRequestFields.PROFILE_DETAILS] = [nutaku.Person.Field.GRADE];
			var req = opensocial.newDataRequest();
			req.add(req.newFetchPersonRequest(opensocial.IdSpec.PersonId.VIEWER, params), "viewer");
			req.send(function(response) {
				var data = {};
			    if (response.hadError()) {
			        console.log("CheckGuestPlayer Error:"+JSON.stringify(response));
			        data.code = -1;
			    } else {
			        var item = response.get("viewer");
			        if (item.hadError()) {
			            console.log("CheckGuestPlayer Error:"+JSON.stringify(response));
			            data.code = -1;
			        } else {
			            var viewer = item.getData();
			            data.code = 0;
			            data.result = viewer.getField(nutaku.Person.Field.GRADE);
			        }
			    }
			    var json = JSON.stringify(data);
			    var bufferSize = lengthBytesUTF8(json) + 1;
				var buffer = _malloc(bufferSize);
				stringToUTF8(json, buffer, bufferSize);
				JsCallCsTest.callback = action;
				Runtime.dynCall('vi', JsCallCsTest.callback, [buffer]);
				});
	  	},
	  	OpenGuestPlayerFrom: function () {
	    	var params = {};
			params[nutaku.GuestRequestFields.VERSION] = 1;

		    var newGuestSignUpResult = opensocial.newGuestSignUp(params);
		    opensocial.requestGuestSignUp(newGuestSignUpResult, function(response) {
			    // Callback processing is performed once the user action on the signup modal is completed.   
			    // However, if the user selects "Sign Up", the user will be directed to the signup page, thus the callback processing is not executed.
			    if (!response.hadError()) {
			        var responseCode = newGuestSignUpResult.getField(nutaku.Guest.Field.RESPONSE_CODE);
			        console.log("OpenGuestPlayerFrom: "+responseCode);
			    }
			    else {
			    //error handling
			    console.log("OpenGuestPlayerFrom Error:"+JSON.stringify(response));
			    }
			});
			    
	  	},
	  	OpenCrossPromotionBanner: function () {
	    	crossPromo.showBannerModal(function(response){
			    SendMessage("NutakuTools","OnOpenCrossPromotionShowBannerModalComplete");
	    	});
	  	},
	  	CrossPromotionTaskArchieve: function(){
	  		crossPromo.achieveTask(archievetaskkey,function(response){
	  			SendMessage("NutakuTools","OnCrossPromotionTaskArchieveResponse",response);
			});
	  	},
	  	CrossPromotionTaskConfirm: function(task_key){
	  		var ctask_key = Pointer_stringify(task_key);
	  		console.log("Confirm task_id "+ctask_key);
	  		crossPromo.confirmTask(ctask_key,function(response){
	  			SendMessage("NutakuTools","OnCrossPromotionTaskConfirmResponse",response);
			});
	  	},OnNutakuToolStart: function(){
	  		var req = opensocial.newDataRequest();
			req.add(req.newFetchPersonRequest(opensocial.IdSpec.PersonId.VIEWER), "viewer");
			req.send(function(response) {
				if (response.hadError()) {
					// error
					console.log('newDataRequest Error:'+JSON.stringify(response));
				} else {
					var item = response.get("viewer");
					if (item.hadError()) {
						// error
						console.log('newDataRequest Error:'+JSON.stringify(response));
					} else {
						var viewer = item.getData();
						var id = viewer.getId();
						SendMessage('NutakuTools','SetUserId', id);
					}
				}
			});
	  		crossPromo.init(tokenxpromo);
			crossPromo.record(function(response){SendMessage("NutakuTools","OnCrossPromoRecordResponse",response);});
	  	}
};
autoAddDeps(LibraryJsCallCsTest, '$JsCallCsTest');
mergeInto(LibraryManager.library, LibraryJsCallCsTest);