import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { APP_BASE_HREF } from '@angular/common';

import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';

import { SearchResultsComponent } from './search-results.component';
import { SearchFilterComponent } from '../search-filter/search-filter.component';
import { ResourceCardComponent } from '../../resource/resource-card/resource-card.component';
import { GuidedAssistantSidebarComponent } from '../../sidebars/guided-assistant-sidebar.component';
import { ServiceOrgSidebarComponent } from '../../sidebars/service-org-sidebar.component';
import { SaveButtonComponent } from '../../resource/user-action/save-button.component';
import { ShareButtonComponent } from '../../resource/user-action/share-button.component';
import { NavigateDataService } from '../../navigate-data.service';
import { WebResourceComponent } from './web-resource/web-resource.component';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { ResourceResult } from './search-result';
import { IResourceFilter, ILuisInput } from './search-results.model';
import { SearchFilterPipe } from '../search-filter.pipe';
import { SearchService } from '../search.service';

describe('SearchResultsComponent', () => {
  let component: SearchResultsComponent;
  let fixture: ComponentFixture<SearchResultsComponent>;
  let pagesToShow: number;
  let searchService: SearchService;


let  isInternalResource: boolean;
let  isWebResource: boolean;
let  isLuisResponse: boolean;
let  searchText: string;
let  searchResults: any;
let  sortType: any;
let  resourceResults: ResourceResult[] = [];
let  filterType: string = 'All';
let  resourceTypeFilter: any[];
let  resourceFilter: IResourceFilter = { ResourceType: '', ContinuationToken: '', TopicIds: '', PageNumber: '', Location: '' };
let  topicIds: any[];
let  isServiceCall: boolean;
let  currentPage: number = 0;

  let loading = false;
  let total: number;
  let page: number;
  let limit: number;
  let offset: number;


  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [
        SearchResultsComponent,
        SearchFilterComponent,
        SearchFilterPipe,
        ResourceCardComponent,
        GuidedAssistantSidebarComponent,
        ServiceOrgSidebarComponent,
        SaveButtonComponent,
        ShareButtonComponent,
        WebResourceComponent,
        PaginationComponent
      ],
      imports: [
        RouterModule.forRoot([
          { path: 'search', component: SearchResultsComponent }
        ]),
        HttpClientModule
      ],
      providers: [NavigateDataService,
        SearchService,
        { provide: APP_BASE_HREF, useValue: '/' }
      ]
    })
      .compileComponents();

  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SearchResultsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // SearchService provided to the TestBed
    searchService = TestBed.get(SearchService);
    
    component.isWebResource = false;
    component.isInternalResource = true;
    component.isLuisResponse = false;
    component.pagesToShow = 10;
    component.searchResults = {};
    component.total = 0;
    component.page = 1;
    component.limit = 0;
    component.offset = 0;
    component.resourceResults = [{}];    
    component.searchResults = {
      resources: {}, webResources: { webPages: { value: {} } }, topIntent: ''   };

  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call filterSearchResults with event defined', () => {
    spyOn(component, 'getInternalResource');
    let event = { filterParam: "All" };
    component.filterSearchResults(event);
    expect(component.getInternalResource).toHaveBeenCalled();
  });

  it('should call filterSearchResults with event undefined', () => {
    spyOn(component, 'getInternalResource');    
    component.filterSearchResults(event);
    expect(component.getInternalResource).toHaveBeenCalledTimes(0);
  });


});
