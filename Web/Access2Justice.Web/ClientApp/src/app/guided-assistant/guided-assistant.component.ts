import { Component, OnInit } from '@angular/core';
import { SearchService } from '../shared/search/search.service';
import { NavigateDataService } from '../shared/navigate-data.service';
import { ILuisInput } from '../shared/search/search-results/search-results.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-guided-assistant',
  templateUrl: './guided-assistant.component.html',
  styleUrls: ['./guided-assistant.component.css']
})
export class GuidedAssistantComponent implements OnInit {
  searchText: string;
  topicLength = 12;
  luisInput: ILuisInput = { Sentence: '', Location: '', TranslateFrom: '', TranslateTo: '', LuisTopScoringIntent: '' };
  guidedAssistantResults: any;

  constructor(
    private searchService: SearchService,
    private router: Router,
    private navigateDataService: NavigateDataService
  ) { }

  onSubmit(guidedAssistantForm) {
    this.luisInput["Sentence"] = guidedAssistantForm.value.searchText;
    this.luisInput["Location"] = JSON.parse(sessionStorage.getItem("globalMapLocation"));
    this.searchService.search(this.luisInput)
      .subscribe(response => {
        this.guidedAssistantResults = response;
        this.navigateDataService.setData(this.guidedAssistantResults);
        this.router.navigateByUrl('/guidedassistantSearch', { skipLocationChange: true });
      });
  }

  ngOnInit() { }
}
