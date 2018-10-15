import { TestBed, inject, tick, fakeAsync } from '@angular/core/testing';
import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { HttpClient, HttpHeaders, HttpParams, HttpClientModule } from '@angular/common/http';
import { PersonalizedPlanService } from '../guided-assistant/personalized-plan/personalized-plan.service';
import { IUserProfile } from '../shared/login/user-profile.model';
import { Global } from '../global';
import { MsalService } from '@azure/msal-angular';
import { Observable } from 'rxjs/Observable';
import { api } from '../../api/api';
import { MSAL_CONFIG } from '@azure/msal-angular/dist/msal.service';
import { RouterTestingModule } from '@angular/router/testing';
import { BroadcastService } from '@azure/msal-angular/dist/broadcast.service';
import { ArrayUtilityService } from '../shared/array-utility.service';
import { ToastrService } from 'ngx-toastr';
import { LoginService } from '../shared/login/login.service';
import { resolve } from 'q';
import { ProfileResolver } from './profile-resolver.service';
import { of } from 'rxjs/observable/of';

describe('ProfileResolverService', () => {
  let profileResolverService: ProfileResolver;
  const httpSpy = jasmine.createSpyObj('http', ['get', 'post', 'put']);
  let global: Global;
  let mockMsalService;
  let mockLoginService;
  let personalizedPlanService: PersonalizedPlanService;
  var originalTimeout;
  let arrayUtilityService: ArrayUtilityService;
  let mockToastr;
  let mockRoute: ActivatedRouteSnapshot;
  let mockState: RouterStateSnapshot;
  
  beforeEach(() => {
    mockToastr = jasmine.createSpyObj(['success']);
    mockMsalService = jasmine.createSpyObj(['getUser']);
    mockLoginService = jasmine.createSpyObj(['upsertUserProfile']);

    TestBed.configureTestingModule({
      imports: [RouterTestingModule, HttpClientModule],
      providers: [ProfileResolver,
        Global,
        BroadcastService,
        PersonalizedPlanService,
        ArrayUtilityService,
        ToastrService,
        { provide: MSAL_CONFIG, useValue: {} },
        { provide: ToastrService, useValue: mockToastr },
        { provide: MsalService, useValue: mockMsalService },
        { provide: LoginService, useValue: mockLoginService }
      ]
    });
    profileResolverService = new ProfileResolver(global, mockMsalService, mockLoginService);
    arrayUtilityService = new ArrayUtilityService();
    httpSpy.get.calls.reset();
  });

  it('should be created', inject([ProfileResolver], (profileResolverService: ProfileResolver) => {
    expect(profileResolverService).toBeDefined();
  }));

  it('should be created', inject([ProfileResolver], (profileResolverService: ProfileResolver) => {
    expect(profileResolverService).toBeTruthy();
  }));

  it('should resolve be called', fakeAsync(() => {
    let mockUserData = {
      displayableId: "mockUser",
      idToken: {
        name: "mockUser",
        preferred_username: "mockUser@microsoft.com"
      },
      identityProvider: "https://login.microsoftonline.com/123test",
      name: "mockUser",
      userIdentifier: "1234567890ABC"
    }

    let mockLoginResponse = {
      oId: "1234567890ABC",
      name: "mockUser",
      eMail: "mockUser@microsoft.com"
    }

    mockMsalService.getUser.and.returnValue(mockUserData);
    mockLoginService.upsertUserProfile.and.returnValue(of(mockLoginResponse));
    profileResolverService.resolve(mockRoute, mockState);
    tick(250);
    expect(profileResolverService.userProfile.name).toEqual("mockUser");
    expect(profileResolverService.userProfile.firstName).toBe("");
    expect(profileResolverService.userProfile.eMail).toEqual("mockUser@microsoft.com");
    expect(mockLoginService.upsertUserProfile).toHaveBeenCalledWith(profileResolverService.userProfile);
  }));
});