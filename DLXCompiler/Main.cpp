#include<stdio.h>
#include<stdlib.h>
#include<string.h>
#include<sys/stat.h>
#define TRUE 1
#define FALSE -1
#define MAX_WORD_LEN 50
#define KEYWORDS_NUM 36
#define UP_NUM 65536
const char * file_name="Test.dlx";
char * src_content;//source content
char * cvt_content;//convert content
enum WORDTYPE{
	ALI,//I-类型算术/逻辑运算
	ALR,//R-类型算术/逻辑运算
	DM,//数据传送
	CB,//条件分支
	TRAP,//TRAP指令
	JMPI,//使用立即数的跳，包括J和JAL
	JMPR,//使用寄存器的跳，包括JR和JALR
	RFE,//从异常返回
	LABEL//    标示 
};
//int find_ALR(const char* word);
//int find_ALI(const char* word);
//const char* ALR_KEYWORDS[]={"ADD","SUB","AND","OR","XOR",
//	"",//由于SHI没有对应的SHR 
//	"SLL","SRL","SRA","SLT","SLE","SEQ","SNE"};
//const char* ALI_KEYWORDS[]={"ADDI","SUBI","ANDI","ORI","XORI","SHI",
//	"SLLI","SRLI","SRAI","SLTI","SLEI","SEQI","SNEI"};

const char* KEYWORDS[KEYWORDS_NUM]={
	         "ADD","ADDI","SUB","SUBI","AND",
			 "ANDI","OR","ORI","XOR","XORI",
			 "",//由于LHI没有对应的LHR,此处留空方便统一判断
			 "LHI","SLL","SLLI","SRL","SRLI",
			 "SRA","SRAI","SLT","SLTI","SLE",
			 "SLEI","SEQ","SEQI","SNE","SNEI",
			 "LW","SW","BEQZ","BNEZ","J",
			 "JR","JAL","JALR","TRAP","XFE"};
void to_upper(const char* src,char * dest){
	char ch;
	int index=0;
	ch=src[index];
	while(ch!=0){
		if(ch>='a' && ch<='z')
			ch-=32;
		dest[index]=ch;
		index++;
		ch=src[index];
	}
	dest[index]=0;
}
enum WORDTYPE get_word_type(const char* src_word){
	char word[MAX_WORD_LEN+1];
	int index=-1;
	int i=0;
	to_upper(src_word,word);
	for(i=0;i<KEYWORDS_NUM;i++){
		if(strcmp(word,KEYWORDS[i])==0){
			index=i;
			break;
		}
	}
	if(index>=0 && index<=25){
		if(index%2==0)
			return ALR;
		else
			return ALI;
	}else if(index==26||index==27){
		return DM;
	}else if(index==28 ||index==29){
		return CB;
	}else if(index==30||index==32){
		return JMPI;
	}else if(index==31||index==33){
		return JMPR;
	}else if(index==34){
		return TRAP;
	}else if(index==35){
		return RFE;
	}
	return LABEL;
}
//int find_ALR(const char* word){
//	int i;
//	for(i=0;i<13;i++){
//		if(strcmp(ALR_KEYWORDS[i],word)==0)
//			return i;
//	}
//	return -1;
//}
//int find_ALI(const char* word){
//	int i;
//	for(i=0;i<13;i++){
//		if(strcmp(ALI_KEYWORDS[i],word)==0)
//			return i;
//	}
//	return -1;
//}
long get_file_size(const char * filename ) { 
	struct stat f_stat; 
	if( stat( filename, &f_stat ) == -1 ){ 
		return -1; 
	} 
	return (long)f_stat.st_size; 
}
int read_file(const char* file_name){
	long f_len;
	FILE *file;
	char ch;
	f_len=get_file_size(file_name);
	//printf("%ld",f_len);
	src_content=(char*)malloc((++f_len)*sizeof(char));
	cvt_content=(char*)malloc((f_len)*sizeof(char));
	if(NULL==src_content || NULL==cvt_content){
		printf("Error when memeory allocation");
		return -1;
	}
	file=fopen(file_name,"r");
	if(NULL==file){
		printf("file open failed");
		return -1;
	}
	int i=0;
	while ((ch=fgetc(file))!=EOF) /* 从文件读一字符，显示到屏幕*/
	{
		src_content[i]=ch;
		i++;
	}
	src_content[i]=0;
	fclose(file);
	return 1;
}
int line=1;
int line_at=0;
int token_index=-1;
char cur_token;
int cvt_index=0;
void push_convert(const char* cvt){
	int i=0;
	char ch;
	ch=cvt[i];
	while(ch!=0)
	{
		if(ch>='a'&&ch<='z')
			ch-=32;
		cvt_content[cvt_index]=ch;
		i++;
		cvt_index++;
		ch=cvt[i];
	}
	cvt_content[cvt_index]=0;
}
char get_src_token(){
	token_index++;
	line_at++;
	cur_token=src_content[token_index];
	if(cur_token=='\n'){
		line++;
		line_at=0;
	}
	return cur_token;
}
void error(){
	printf("error at char \'%c\'(%d), position %d, line %d",cur_token,cur_token,line_at,line);
	system("pause");
	exit(1);

}
int is_validate_token(char ch){
	if(ch>='A' && ch<='Z')
		return TRUE;
	if(ch>='a' && ch<='z')
		return TRUE;
	if(ch>='0' && ch<='9')
		return TRUE;
	if(ch=='_')
		return TRUE;
	return FALSE;
}
void get_word(char* word){
	int i;
	i=0;
	while(cur_token!=' ' && cur_token!=0 && cur_token!='\n' && cur_token!='\r'){
		if(is_validate_token(cur_token)==FALSE){
			if(cur_token==':')
				break;
			error();
		}
		else{
			word[i]=cur_token;
			i++;
			if(i>=MAX_WORD_LEN)
			{
				error();
				break;
			}
			get_src_token();
		}
	}
	word[i]=0;
	//printf("got word %s\n\n\n",word);
}
int get_number(){
	int rtn=0;
	int is_neg=FALSE;
    if(cur_token=='x'){
		get_src_token();
		while(1){
			if(cur_token>='0' && cur_token<='9'){
				rtn*=16;
				rtn+=cur_token-'0';
			}else if(cur_token>='a' && cur_token<='f'){
				rtn*=16;
				rtn+=cur_token-'a'+10;
			}else if(cur_token>='A' && cur_token<='F'){
				rtn*=16;
				rtn+=cur_token-'A'+10;
			}else
				break;
			get_src_token();
		}
		if(rtn>32767)
			rtn-=65536;
		return rtn;
	}
	if(cur_token=='-'){
		is_neg=TRUE;
		get_src_token();
	}
	while(cur_token>='0' && cur_token<='9'){
		rtn*=10;
		rtn+=(cur_token-'0');
		get_src_token();
	}
	if(is_neg==TRUE)
		rtn=-rtn;
	return rtn;
}
void skip_space(){
	while(cur_token==' ' || cur_token=='\t')
		get_src_token();
}
void S();//sentence
void I();//instruction include comment
void I_ALI();
void I_ALR();
void I_DM();
void I_CB();
void I_JMPI();
void I_JMPR();
void I_TRAP();
void I_RFE();
void R();//registers
void D();//dlx programm
void C();//comment
void D(){
	while(cur_token!=0){
		switch(cur_token){
		case '\'':
			C();
			continue;
			break;
		case '/':
			get_src_token();
			if(cur_token!='/')
				error();
			else
				C();
			continue;
			break;
		case ' ':
		case '\t':
			skip_space();
			continue;
			break;
		case '\n':
		case '\r':
			get_src_token();//ignore '\r', as each sentence end with "\n\r" in windows
			break;
		default:
			if(is_validate_token(cur_token)==TRUE){
				I();
				skip_space();
				if(cur_token=='\n' || cur_token=='\'' || cur_token=='/')
					break;
				else
					error();
			}else
				error();
			break;  
		}


	}
}
void I(){
	char word[MAX_WORD_LEN+1];
	enum WORDTYPE w_type;

	get_word(word);
	//printf("got word %s\n\n",word);
	w_type=get_word_type(word);
	if(w_type==LABEL){

		//do something...
		printf("got label :\"%s\"\n",word);
		push_convert(word);

		skip_space();

		if(cur_token!=':')
			error();
		push_convert(":");

		get_src_token();
		skip_space();
		get_word(word);
		w_type=get_word_type(word);
		if(w_type==LABEL)
			error();


	}
	
	push_convert(word);
	push_convert(" ");
	switch(w_type){
	case ALR:
		I_ALR();
		break;
	case ALI:
		I_ALI();
		break;
	case DM://数据传送
		I_DM();
		break;
	case CB:
		I_CB();
		break;
	case JMPI:
		I_JMPI();
		break;
	case TRAP:
		I_TRAP();
		break;
	case JMPR:
		I_JMPR();
		break;
	default:
		error();
		break;
	}
	push_convert("\n");

}
void I_ALR(){
	skip_space();
	R();
	skip_space();
	if(cur_token!=',')
		error();
	get_src_token();
	skip_space();
	R();
	skip_space();
	if(cur_token!=',')
		error();
	get_src_token();
	skip_space();
	R();
}
void I_ALI(){
	skip_space();
	R();
	skip_space();
	if(cur_token!=',')
		error();
	get_src_token();
	skip_space();
	R();
	skip_space();
	if(cur_token!=',')
		error();
	get_src_token();
	skip_space();
	printf("got %d\n",get_number());
}
void I_DM(){
	skip_space();
	R();
	skip_space();
	if(cur_token!=',')
		error();
	get_src_token();
	skip_space();
	R();
	skip_space();
	if(cur_token!=',')
		error();
	get_src_token();
	skip_space();
	printf("got offset %d\n",get_number());
}
void I_CB(){
	char label[50];
	skip_space();
	R();
	skip_space();
	if(cur_token!=',')
		error();
	get_src_token();
	skip_space();
	get_word(label);
	push_convert(label);
	printf("c b to %s\n",label);
}
void I_JMPI(){
	char label[50];
	skip_space();
	get_word(label);
	push_convert(label);
}
void I_TRAP(){
	skip_space();
	printf("trap %d\n",get_number());
}
void I_JMPR(){
	skip_space();
	R();
}
void I_RFE(){
	
}
void R(){
	int r_id;
	if(cur_token!='r' && cur_token!='R')
		error();
	get_src_token();
	r_id=get_number();
	if(r_id<0 || r_id>31)
		error();
	push_convert("R ");
	return;   
}
void TI(){

}
void C(){
	printf("\nComment:");
	char ch=get_src_token();
	putchar(ch);
	while(ch!='\n' && ch!=0){
		ch= get_src_token();
		putchar(ch);
	}
	putchar('\n');
	return;
}
int main(){
	if(read_file(file_name)==-1)
		exit(1);
	token_index=-1;
	get_src_token();
	D();
	printf("Success!\n\n");
	printf("%s",cvt_content);
	free(src_content);
	free(cvt_content);
	system("pause");
}
