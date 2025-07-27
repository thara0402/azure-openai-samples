using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace travel_concierge.Agents
{
    public class DestinationSuggestAgent(ILogger<DestinationSuggestAgent> logger)
    {
        private readonly ILogger<DestinationSuggestAgent> _logger = logger;

        [Function(nameof(GetDestinationSuggest))]
        public string GetDestinationSuggest(
            [McpToolTrigger("get_destination_suggest", "��]�̍s����ɋ��߂���������R����ŗ^����ƁA�������߂̗��s����Ă��܂��B")] ToolInvocationContext context,
            [McpToolProperty("searchTerm", "string", "�s����ɋ��߂��]�̏���")] string searchTerm)
        {
            // This is sample code. Replace this with your own logic.
            var result = $"""
            {searchTerm}�̏����ł������߂̗��s����Ă��܂��B�D�݂ɉ����đI��ł��������B
            ### ����
            1. **����{��**  
            - �����x�̍����r�[�`�A�񗢏�A����C�����قȂǊό��������L�x�B
            - �~�ł��g�����A�����b�N�X�������͋C���y���߂�B

            2. **�Ί_���E�{�Ó�**  
            - �썑�炵�����������R���L����A�_�C�r���O��V���m�[�P�����O���l�C�B
            - ���Ȃ�ł͂̋��y�������y���߂�B

            3. **�������E�����哇**  
            - �����̍����Ē��Ⓡ�S�A���L�̎��R���𖞋i�B
            - ���M�т̕��͋C���y���߂�B

            ---

            ### �C�O
            1. **�n���C�i�I�A�t����}�E�C���j**  
            - �N�Ԃ�ʂ��ĉ��K�ȋC���B�r�[�`���]�[�g��g���b�L���O�ȂǑ��l�ȃA�N�e�B�r�e�B���\�B
            - ���{��Ή����[�����Ă��Ĉ��S�B

            2. **�^�C�i�v�[�P�b�g��N���r�j**  
            - �荠�ȉ��i�Ŋy���߂郊�]�[�g�n�B�������C��ƃ^�C���������́B
            - �ό������Ⓡ������������߁B

            3. **�o�����i�C���h�l�V�A�j**  
            - �������]�[�g�����y�ȏh���{�݂܂őI�������L���B
            - �q���Y�[�����Ǝ��R���D��Ȃ����j�[�N�ȕ��͋C�����\�B

            4. **�I�[�X�g�����A�i�P�A���Y��S�[���h�R�[�X�g�j**  
            - �O���[�g�o���A���[�t�ł̊C�m�A�N�e�B�r�e�B���l�C�B
            - �g�����C��Ŏ��R�Ɠs�s�ό����o�����X�ǂ��y���߂�B

            5. **�O�A����T�C�p��**  
            - ���{����߂��A�Z���Ԃł��y���߂�썑���]�[�g�B
            - �̂�т�߂����������ɍœK�B

            �ǂ���g�����C����y���߂�ꏊ�ł��B�\�Z�◷�s���Ԃɍ��킹�Ă��I�т��������I
            """;

            return result;
        }
    }
}